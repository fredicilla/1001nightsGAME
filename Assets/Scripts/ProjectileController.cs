using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public float lifetime = 5f;
    public GameObject owner;
    private float ignoreOwnerDuration = 0.5f;
    private float spawnTime;
    
    private void Start()
    {
        spawnTime = Time.time;
        Destroy(gameObject, lifetime);
        
        // Ignore collision with owner for first 0.5 seconds
        if (owner != null)
        {
            Collider ownerCollider = owner.GetComponent<Collider>();
            Collider myCollider = GetComponent<Collider>();
            
            if (ownerCollider != null && myCollider != null)
            {
                Physics.IgnoreCollision(myCollider, ownerCollider, true);
                Debug.Log($"🍎 Projectile ignoring collision with owner: {owner.name}");
            }
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        GameObject hitObject = collision.gameObject;
        
        // Don't hit the owner in first 0.5 seconds
        if (hitObject == owner && Time.time - spawnTime < ignoreOwnerDuration)
        {
            Debug.Log($"🛡️ Projectile ignoring owner {owner.name} (too soon)");
            return;
        }
        
        if (hitObject.CompareTag("Player") || hitObject.CompareTag("Monster"))
        {
            HealthSystem health = hitObject.GetComponent<HealthSystem>();
            if (health != null)
            {
                Vector3 damageDirection = (hitObject.transform.position - transform.position).normalized;
                health.Die(damageDirection);
            }
        }
        
        Destroy(gameObject);
    }
}
