using UnityEngine;
using UnityEngine.InputSystem;
using GeniesGambit.Core;
using GeniesGambit.Level;

namespace GeniesGambit.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] float moveSpeed = 7f;
        [SerializeField] float jumpForce = 14f;
        [SerializeField] float coyoteTime = 0.12f;

        [Header("Ground Detection")]
        [SerializeField] Transform groundCheck;
        [SerializeField] float groundRadius = 0.12f;
        [SerializeField] LayerMask groundLayer;

        [Header("Sprite")]
        [SerializeField] bool invertFlip = false; // Set true for enemy (sprite faces left by default)

        InputAction _moveAction;
        InputAction _jumpAction;
        Rigidbody2D _rb;
        SpriteRenderer _sr;
        Animator _animator;
        MovementRecorder _recorder;
        float _horizontal;
        bool _jumpPressed;
        bool _isGrounded;
        float _coyoteCounter;
        bool _active = true;
        bool _isFallingRespawn = false;  // prevents repeated restart calls while falling
        PlayerInput _playerInput;

        /// <summary>True while this character accepts player input AND has an active PlayerInput component.
        /// Only the character currently being controlled by the human player will return true.</summary>
        public bool IsActive => _active && _playerInput != null && _playerInput.isActiveAndEnabled;
        Vector3 _startPosition;
        float _weightSlowdown = 0f;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
            _recorder = GetComponent<MovementRecorder>();
            _playerInput = GetComponent<PlayerInput>();

            _startPosition = transform.position;

            if (_playerInput != null && _playerInput.actions != null)
            {
                _moveAction = _playerInput.actions.FindAction("Move");
                _jumpAction = _playerInput.actions.FindAction("Jump");

                if (_moveAction != null) _moveAction.Enable();
                if (_jumpAction != null)
                {
                    _jumpAction.Enable();
                    _jumpAction.performed += OnJumpPerformed;
                }
            }
        }

        void OnDestroy()
        {
            if (_jumpAction != null)
                _jumpAction.performed -= OnJumpPerformed;
        }

        void OnEnable() => GameManager.OnStateChanged += HandleStateChange;
        void OnDisable() => GameManager.OnStateChanged -= HandleStateChange;

        void OnJumpPerformed(InputAction.CallbackContext ctx)
        {
            Debug.Log($"[Player] Jump input received. Active: {_active}");
            if (!_active) return;
            _jumpPressed = true;
        }

        void Update()
        {
            if (!_active) return;

            if (_moveAction != null)
                _horizontal = _moveAction.ReadValue<Vector2>().x;

            bool wasGrounded = _isGrounded;
            _isGrounded = Physics2D.OverlapCircle(
                groundCheck.position, groundRadius, groundLayer);

            if (!wasGrounded && _isGrounded)
            {
                Debug.Log("[Player] Landed on ground");
                Core.AudioManager.Play(Core.AudioManager.SoundID.Land);
            }

            if (_isGrounded) _coyoteCounter = coyoteTime;
            else _coyoteCounter -= Time.deltaTime;

            // Flip sprite based on movement direction
            if (invertFlip)
            {
                // Inverted: sprite faces left by default
                if (_horizontal > 0.01f) _sr.flipX = true;
                if (_horizontal < -0.01f) _sr.flipX = false;
            }
            else
            {
                // Normal: sprite faces right by default
                if (_horizontal > 0.01f) _sr.flipX = false;
                if (_horizontal < -0.01f) _sr.flipX = true;
            }

            if (_animator != null)
            {
                _animator.SetFloat("Speed", Mathf.Abs(_rb.linearVelocity.x));
            }

            if (transform.position.y < -10f && !_isFallingRespawn)
            {
                _isFallingRespawn = true;  // block repeated calls from subsequent frames
                Debug.Log("[Player] Fell off the map! Respawning...");
                Core.AudioManager.Play(Core.AudioManager.SoundID.Fall);

                var iterationManager = FindFirstObjectByType<Core.IterationManager>();
                if (iterationManager != null && iterationManager.CurrentIteration == 1)
                {
                    // Iteration 1: full reset clears all recordings and starts fresh
                    iterationManager.ResetIterations();
                }
                else if (iterationManager != null)
                {
                    // All other iterations: restart current iteration so ghosts replay again
                    iterationManager.RestartCurrentIteration();
                }
                else
                {
                    _isFallingRespawn = false;  // simple respawn — allow re-trigger if needed
                    RespawnAtStart();

                    var coinSpawner = FindFirstObjectByType<Level.CoinSpawner>();
                    if (coinSpawner != null)
                        coinSpawner.ResetAllCoins();

                    var ghost = FindFirstObjectByType<Enemies.ChasingMonster>();
                    if (ghost != null)
                        ghost.RespawnGhostPublic();
                }
            }
        }

        void FixedUpdate()
        {
            if (!_active)
            {
                Debug.Log("[Player] FixedUpdate - Not active");
                return;
            }

            float effectiveSpeed = Mathf.Max(moveSpeed - _weightSlowdown, moveSpeed * 0.3f);
            _rb.linearVelocity = new Vector2(_horizontal * effectiveSpeed, _rb.linearVelocity.y);

            if (_jumpPressed)
            {
                Debug.Log($"[Player] Jump pressed! Grounded: {_isGrounded}, CoyoteCounter: {_coyoteCounter}");
            }

            if (_jumpPressed && _coyoteCounter > 0f)
            {
                Debug.Log($"[Player] Applying jump force: {jumpForce}");
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
                _coyoteCounter = 0f;
                Core.AudioManager.Play(Core.AudioManager.SoundID.Jump);
                Debug.Log($"[Player] Jump applied! New velocity: {_rb.linearVelocity}");
            }
            _jumpPressed = false;
        }

        void HandleStateChange(GameState old, GameState next)
        {
            _active = next == GameState.HeroTurn || next == GameState.MonsterTurn;

            // Clear the falling-respawn guard whenever a new turn/round starts
            if (next == GameState.HeroTurn || next == GameState.MonsterTurn)
                _isFallingRespawn = false;

            Debug.Log($"[Player] HandleStateChange: {old} → {next}, _active = {_active}");

            if (next == GameState.HeroTurn)
            {
                if (_playerInput != null && _playerInput.actions != null)
                {
                    // Unsubscribe old handler (prevent double subscription)
                    if (_jumpAction != null)
                        _jumpAction.performed -= OnJumpPerformed;

                    // Get fresh references using safe FindAction
                    _moveAction = _playerInput.actions.FindAction("Move");
                    _jumpAction = _playerInput.actions.FindAction("Jump");

                    // Explicitly enable — FindAction does NOT auto-enable the action map
                    if (_moveAction != null) _moveAction.Enable();
                    if (_jumpAction != null)
                    {
                        _jumpAction.Enable();
                        _jumpAction.performed += OnJumpPerformed;
                    }

                    // Re-activate input without toggling enabled (toggling causes OnDisable/OnEnable)
                    _playerInput.ActivateInput();

                    Debug.Log($"[Player] Input re-activated. MoveAction: {_moveAction != null}, JumpAction: {_jumpAction != null}");
                }
                else
                {
                    Debug.LogWarning("[Player] PlayerInput or actions is null during HeroTurn!");
                }

                if (old == GameState.GenieWishScreen)
                    RespawnAtStart();
            }

            if (!_active) _rb.linearVelocity = Vector2.zero;
        }

        public void ResetFallGuard()
        {
            _isFallingRespawn = false;
        }

        void RespawnAtStart()
        {
            transform.position = _startPosition;
            _rb.linearVelocity = Vector2.zero;
            _horizontal = 0f;
            _jumpPressed = false;
            _weightSlowdown = 0f;
            KeyCollectible.ResetKey();
            CoinCollectible.ResetCoins();
        }

        public void ApplyWeightSlowdown(float totalSlowdown)
        {
            _weightSlowdown = totalSlowdown;
            Debug.Log($"[Player] Weight slowdown applied: {_weightSlowdown}");
        }

        void OnDrawGizmosSelected()
        {
            if (groundCheck == null) return;
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }


    }
}