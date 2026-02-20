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
        string _targetTag;
        float _speed;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void Initialize(Vector3 direction, float speed, string targetTag)
        {
            _speed = speed;
            _targetTag = targetTag;
            _rb.linearVelocity = direction.normalized * speed;
            
            Debug.Log($"[Projectile] {gameObject.name} initialized. Target tag: '{targetTag}', Direction: {direction}, Speed: {speed}");
            
            Destroy(gameObject, lifetime);
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            Debug.Log($"[Projectile] {gameObject.name} collided with {collision.name} (Tag: '{collision.tag}'). Looking for target tag: '{_targetTag}'");
            
            if (collision.CompareTag(_targetTag))
            {
                var health = collision.GetComponent<Health>();
                if (health != null)
                {
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
    }
}
