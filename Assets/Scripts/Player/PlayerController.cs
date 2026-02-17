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
        float  _horizontal;
        bool   _jumpPressed;
        bool   _isGrounded;
        float  _coyoteCounter;
        bool   _active = true;
        Vector3 _startPosition;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponent<SpriteRenderer>();
            
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
            if (!_active) return;
            _jumpPressed = true;
        }

        void Update()
        {
            if (!_active) return;
            
            if (_moveAction != null)
                _horizontal = _moveAction.ReadValue<Vector2>().x;
            
            _isGrounded = Physics2D.OverlapCircle(
                groundCheck.position, groundRadius, groundLayer);

            if (_isGrounded) _coyoteCounter = coyoteTime;
            else             _coyoteCounter -= Time.deltaTime;

            if (_horizontal >  0.01f) _sr.flipX = false;
            if (_horizontal < -0.01f) _sr.flipX = true;
        }

        void FixedUpdate()
        {
            if (!_active) return;
            _rb.linearVelocity = new Vector2(_horizontal * moveSpeed, _rb.linearVelocity.y);

            if (_jumpPressed && _coyoteCounter > 0f)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
                _coyoteCounter = 0f;
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
            KeyCollectible.ResetKey();
        }

        void OnDrawGizmosSelected()
        {
            if (groundCheck == null) return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
    }
}