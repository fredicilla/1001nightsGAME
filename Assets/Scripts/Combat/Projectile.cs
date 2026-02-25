using UnityEngine;

namespace GeniesGambit.Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] float lifetime = 5f;
        [SerializeField] int damage = 1;

        Rigidbody2D _rb;
        Collider2D _collider;
        string _targetTag;
        float _speed;
        float _debugTimer = 0f;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();

            // Ensure collider is set as trigger for OnTriggerEnter2D to work
            if (_collider != null)
            {
                _collider.isTrigger = true;
            }

            // Force Continuous collision detection to prevent tunneling
            if (_rb != null)
            {
                _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }
        }

        void Update()
        {
            // Periodic debug check to see if there's a target nearby
            _debugTimer += Time.deltaTime;
            if (_debugTimer >= 0.8f && !string.IsNullOrEmpty(_targetTag))
            {
                _debugTimer = 0f;
                var target = GameObject.FindWithTag(_targetTag);
                if (target != null)
                {
                    float dist = Vector3.Distance(transform.position, target.transform.position);
                    if (dist < 3f)
                    {
                        var targetCollider = target.GetComponent<Collider2D>();
                        Debug.Log($"[Projectile] Target '{target.name}' dist={dist:F2}, collider={targetCollider != null}, enabled={targetCollider?.enabled}, isTrigger={targetCollider?.isTrigger}, myPos={transform.position}, targetPos={target.transform.position}");
                    }
                }
            }
        }

        Vector3 _previousPosition;

        void Start()
        {
            _previousPosition = transform.position;
        }

        void FixedUpdate()
        {
            // Raycast-based collision detection to prevent tunneling through targets
            if (string.IsNullOrEmpty(_targetTag)) return;

            Vector3 currentPos = transform.position;
            Vector3 direction = currentPos - _previousPosition;
            float distance = direction.magnitude;

            // Use layer mask that queries all layers (not just default collision matrix)
            int allLayers = ~0; // All layers

            if (distance > 0.01f)
            {
                // Use CircleCast for better detection instead of simple Raycast
                float colliderRadius = _collider != null ? (_collider as CircleCollider2D)?.radius ?? 0.15f : 0.15f;
                RaycastHit2D[] hits = Physics2D.CircleCastAll(_previousPosition, colliderRadius, direction.normalized, distance, allLayers);
                foreach (var hit in hits)
                {
                    if (hit.collider != _collider && hit.collider.CompareTag(_targetTag))
                    {
                        Debug.Log($"[Projectile] CIRCLECAST hit {hit.collider.name} (tag: {hit.collider.tag})!");
                        HandleCollision(hit.collider);
                        return;
                    }
                }
            }

            // Also check overlap at current position as fallback (increased radius from 0.8 to 1.0)
            Collider2D[] overlaps = Physics2D.OverlapCircleAll(transform.position, 1.0f, allLayers);
            foreach (var hit in overlaps)
            {
                if (hit != _collider && hit.CompareTag(_targetTag))
                {
                    Debug.Log($"[Projectile] FixedUpdate overlap with {hit.name} (tag: {hit.tag})!");
                    HandleCollision(hit);
                    return;
                }
            }

            _previousPosition = currentPos;
        }

        public void Initialize(Vector3 direction, float speed, string targetTag, Collider2D shooterCollider = null)
        {
            _speed = speed;
            _targetTag = targetTag;
            _previousPosition = transform.position;

            // Enable continuous collision detection to prevent tunneling
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rb.linearVelocity = direction.normalized * speed;

            // Ignore collision with the shooter so projectile doesn't hit itself
            if (shooterCollider != null && _collider != null)
            {
                Physics2D.IgnoreCollision(_collider, shooterCollider);
                Debug.Log($"[Projectile] Ignoring collision with shooter: {shooterCollider.gameObject.name}");
            }

            Debug.Log($"[Projectile] {gameObject.name} initialized. Target tag: '{targetTag}', Direction: {direction}, Speed: {speed}");

            Destroy(gameObject, lifetime);
        }

        public void IgnoreCollider(Collider2D colliderToIgnore)
        {
            if (colliderToIgnore != null && _collider != null)
            {
                Physics2D.IgnoreCollision(_collider, colliderToIgnore);
            }
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            HandleCollision(collision);
        }

        void OnTriggerStay2D(Collider2D collision)
        {
            // Additional fallback for cases where OnTriggerEnter2D misses
            HandleCollision(collision);
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            // Also handle non-trigger collisions
            HandleCollision(collision.collider);
        }

        void HandleCollision(Collider2D collision)
        {
            Debug.Log($"[Projectile] {gameObject.name} collided with {collision.name} (Tag: '{collision.tag}'). Looking for target tag: '{_targetTag}'");

            if (collision.CompareTag(_targetTag))
            {
                var health = collision.GetComponent<Health>();
                if (health != null)
                {
                    Debug.Log($"[Projectile] Found Health on {collision.name}, isDead={health.IsDead}");
                    health.TakeDamage(damage);
                    Debug.Log($"[Projectile] Hit {collision.name} for {damage} damage!");
                }
                else
                {
                    Debug.LogWarning($"[Projectile] {collision.name} has correct tag '{_targetTag}' but NO Health component!");
                }
                Destroy(gameObject);
            }
            else if (collision.CompareTag("Ground") || collision.CompareTag("Platforms"))
            {
                Debug.Log($"[Projectile] Hit ground/platform, destroying");
                Destroy(gameObject);
            }
            else
            {
                Debug.Log($"[Projectile] Collision ignored - tag mismatch (got '{collision.tag}', want '{_targetTag}')");
            }
        }

        void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            // Draw the movement path
            if (_previousPosition != Vector3.zero)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(_previousPosition, transform.position);
            }

            // Draw the overlap circle
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 1.0f);

            // Draw the collider radius
            if (_collider != null && _collider is CircleCollider2D circleCollider)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, circleCollider.radius);
            }

            // Draw line to target if nearby
            if (!string.IsNullOrEmpty(_targetTag))
            {
                var target = GameObject.FindWithTag(_targetTag);
                if (target != null)
                {
                    float dist = Vector3.Distance(transform.position, target.transform.position);
                    if (dist < 3f)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawLine(transform.position, target.transform.position);
                    }
                }
            }
        }
    }
}