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
        [SerializeField] float moveSpeed  = 7f;
        [SerializeField] float jumpForce  = 14f;
        [SerializeField] float coyoteTime = 0.12f;

        [Header("Ground Detection")]
        [SerializeField] Transform  groundCheck;
        [SerializeField] float      groundRadius = 0.12f;
        [SerializeField] LayerMask  groundLayer;

        InputAction _moveAction;
        InputAction _jumpAction;
        Rigidbody2D    _rb;
        SpriteRenderer _sr;
        Animator       _animator;
        float  _horizontal;
        bool   _jumpPressed;
        bool   _isGrounded;
        float  _coyoteCounter;
        bool   _active = true;
        Vector3 _startPosition;
        float _weightSlowdown = 0f;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
            
            _startPosition = transform.position;
            
            var playerInput = GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                _moveAction = playerInput.actions["Move"];
                _jumpAction = playerInput.actions["Jump"];
                
                if (_jumpAction != null)
                    _jumpAction.performed += OnJumpPerformed;
            }
        }

        void OnDestroy()
        {
            if (_jumpAction != null)
                _jumpAction.performed -= OnJumpPerformed;
        }

        void OnEnable()  => GameManager.OnStateChanged += HandleStateChange;
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
            }

            if (_isGrounded) _coyoteCounter = coyoteTime;
            else             _coyoteCounter -= Time.deltaTime;

            if (_horizontal >  0.01f) _sr.flipX = false;
            if (_horizontal < -0.01f) _sr.flipX = true;
            
            if (_animator != null)
            {
                _animator.SetFloat("Speed", Mathf.Abs(_rb.linearVelocity.x));
            }
            
            if (transform.position.y < -10f)
            {
                Debug.Log("[Player] Fell off the map! Respawning...");
                RespawnAtStart();
                
                var coinSpawner = FindFirstObjectByType<Level.CoinSpawner>();
                if (coinSpawner != null)
                    coinSpawner.ResetAllCoins();
                
                var ghost = FindFirstObjectByType<Enemies.ChasingMonster>();
                if (ghost != null)
                    ghost.RespawnGhostPublic();
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
                Debug.Log($"[Player] Jump applied! New velocity: {_rb.linearVelocity}");
            }
            _jumpPressed = false;
        }

        void HandleStateChange(GameState old, GameState next)
        {
            _active = next == GameState.HeroTurn || next == GameState.MonsterTurn;
            
            if (next == GameState.HeroTurn && old == GameState.GenieWishScreen)
            {
                RespawnAtStart();
            }
            
            if (!_active) _rb.linearVelocity = Vector2.zero;
        }

        void RespawnAtStart()
        {
            transform.position = _startPosition;
            _rb.linearVelocity = Vector2.zero;
            _horizontal = 0;
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

        void OnGUI()
        {
            if (!Application.isPlaying) return;
            
            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            style.normal.textColor = Color.white;
            
            GUI.Label(new Rect(10, 10, 400, 30), $"Grounded: {_isGrounded}", style);
            GUI.Label(new Rect(10, 40, 400, 30), $"CoyoteCounter: {_coyoteCounter:F2}", style);
            GUI.Label(new Rect(10, 70, 400, 30), $"Active: {_active}", style);
            GUI.Label(new Rect(10, 100, 400, 30), $"Velocity: {_rb.linearVelocity}", style);
            GUI.Label(new Rect(10, 130, 400, 30), $"Jump Pressed: {_jumpPressed}", style);
        }
    }
}