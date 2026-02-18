using UnityEngine;
using GeniesGambit.Core;

namespace GeniesGambit.Enemies
{
    public class ChasingMonster : MonoBehaviour
    {
        [Header("Chase Settings")]
        [SerializeField] float chaseSpeed = 3f;
        [SerializeField] float detectionRange = 15f;
        [SerializeField] float catchDistance = 0.5f;

        [Header("References")]
        [SerializeField] Transform player;
        [SerializeField] SpriteRenderer spriteRenderer;

        bool _isChasing = false;
        Rigidbody2D _rb;
        Vector3 _spawnPosition = new Vector3(6.38f, 5f, 0f);

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
            if (!_isChasing || player == null) return;
            if (!IsGameActive())
            {
                _rb.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 direction = (player.position - transform.position).normalized;
            _rb.linearVelocity = direction * chaseSpeed;

            if (spriteRenderer != null)
                spriteRenderer.flipX = direction.x > 0;

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= catchDistance)
                CatchPlayer();
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
            
            player.position = new Vector3(-6.123f, 2.278f, 0);
            
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

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, catchDistance);
        }
    }
}
