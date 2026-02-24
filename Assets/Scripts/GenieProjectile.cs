using UnityEngine;

public class GenieProjectile : MonoBehaviour
{
    private Vector3 targetPosition;
    private float speed = 8f;
    private int damage = 1;
    private float lifetime = 5f;
    private GameObject owner;
    private Rigidbody rb;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, lifetime);
    }
    
    public void Initialize(Vector3 target, float projectileSpeed, GameObject projectileOwner)
    {
        targetPosition = target;
        speed = projectileSpeed;
        owner = projectileOwner;
        
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        
        if (rb != null)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            rb.linearVelocity = direction * speed;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == owner) return;
        
        if (other.CompareTag("Player"))
        {
            HealthSystem health = other.GetComponent<HealthSystem>();
            if (health != null)
            {
                health.TakeDamage(damage, transform.position);
            }
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
