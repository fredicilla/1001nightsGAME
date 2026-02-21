using UnityEngine;
using UnityEngine.InputSystem;
using GeniesGambit.Core;

namespace GeniesGambit.Enemies
{
    public enum MonsterControlMode
    {
        AIControlled,
        PlayerControlled
    }

    public class ChasingMonster : MonoBehaviour
    {
        [Header("Chase Settings")]
        [SerializeField] float chaseSpeed = 3f;
        [SerializeField] float detectionRange = 15f;
        [SerializeField] float catchDistance = 0.5f;
        [SerializeField] float playerMoveSpeed = 4f;

        [Header("References")]
        [SerializeField] Transform player;
        [SerializeField] SpriteRenderer spriteRenderer;

        bool _isChasing = false;
        Rigidbody2D _rb;
        Vector3 _spawnPosition = new Vector3(6.38f, 5f, 0f);
        MonsterControlMode _controlMode = MonsterControlMode.AIControlled;
        InputAction _moveAction;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void OnEnable()
        {
            GameManager.OnStateChanged += HandleStateChange;
        }

        void OnDisable()
        {
            GameManager.OnStateChanged -= HandleStateChange;
        }

        void Start()
        {
            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                    player = playerObj.transform;
            }
        }

        void HandleStateChange(GameState oldState, GameState newState)
        {
            if (newState == GameState.GenieWishScreen)
            {
                RespawnGhost();
            }
        }

        void Update()
        {
            if (player == null) return;
            if (!IsGameActive()) return;

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= detectionRange)
                _isChasing = true;
        }

        void FixedUpdate()
        {
            if (!IsGameActive())
            {
                _rb.linearVelocity = Vector2.zero;
                return;
            }

            if (_controlMode == MonsterControlMode.AIControlled)
            {
                DoAIMovement();
            }
            else
            {
                DoPlayerMovement();
            }
        }

        void DoAIMovement()
        {
            if (!_isChasing || player == null) return;

            Vector2 direction = (player.position - transform.position).normalized;
            _rb.linearVelocity = direction * chaseSpeed;

            if (spriteRenderer != null)
                spriteRenderer.flipX = direction.x > 0;

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= catchDistance)
                CatchPlayer();
        }

        void DoPlayerMovement()
        {
            if (_moveAction == null) return;

            float horizontal = _moveAction.ReadValue<Vector2>().x;
            _rb.linearVelocity = new Vector2(horizontal * playerMoveSpeed, _rb.linearVelocity.y);

            if (horizontal > 0.01f) spriteRenderer.flipX = false;
            if (horizontal < -0.01f) spriteRenderer.flipX = true;
        }

        bool IsGameActive()
        {
            if (GameManager.Instance == null) return true;

            var state = GameManager.Instance.CurrentState;
            return state == GameState.HeroTurn || state == GameState.MonsterTurn;
        }

        void CatchPlayer()
        {
            Debug.Log("[Monster] Caught the player! Respawning both...");

            var playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
                playerRb.linearVelocity = Vector2.zero;

            player.position = new Vector3(-7.5f, 2.278f, 0);

            var playerController = player.GetComponent<Player.PlayerController>();
            if (playerController != null)
                playerController.ApplyWeightSlowdown(0f);

            Level.KeyCollectible.ResetKey();
            Level.CoinCollectible.ResetCoins();

            var coinSpawner = FindFirstObjectByType<Level.CoinSpawner>();
            if (coinSpawner != null)
                coinSpawner.ResetAllCoins();

            RespawnGhost();
        }

        void RespawnGhost()
        {
            transform.position = _spawnPosition;
            _rb.linearVelocity = Vector2.zero;
            _isChasing = false;
        }

        public void RespawnGhostPublic()
        {
            RespawnGhost();
        }

        public void SetControlMode(MonsterControlMode mode)
        {
            _controlMode = mode;
            Debug.Log($"[Monster] Control mode set to: {mode}");

            if (mode == MonsterControlMode.PlayerControlled)
            {
                var playerInput = FindFirstObjectByType<UnityEngine.InputSystem.PlayerInput>();
                if (playerInput != null)
                {
                    _moveAction = playerInput.actions["Move"];
                }
            }
            else
            {
                _moveAction = null;
            }
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.gameObject.CompareTag("Player")) return;

            if (_controlMode == MonsterControlMode.PlayerControlled)
            {
                if (collision.gameObject.GetComponent<Player.GhostReplay>() != null)
                {
                    Debug.Log("[Monster] Caught the ghost!");
                    // NOTE: ChasingMonster is for the Wife wish, not the iteration system
                    // The iteration system uses Health component for ghost death
                }
            }
            else
            {
                CatchPlayer();
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, catchDistance);
        }
    }
}
