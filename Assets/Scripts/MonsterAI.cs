using UnityEngine;

public class MonsterAI : MonoBehaviour
{
    [Header("AI Settings")]
    public Transform player;
    public float moveSpeed = 1.5f;
    public float detectionRange = 20f;
    public float attackRange = 1.5f;
    public int damage = 1;
    public int maxHealth = 1;
    public bool isDead = false;
    
    [Header("References")]
    public GenieBossController genieBoss;
    
    private Rigidbody rb;
    private HealthSystem healthSystem;
    private Animator animator;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.freezeRotation = true;
        }
        
        animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            Debug.Log($"✅ Monster Animator found: {animator.gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"⚠️ No Animator found on {gameObject.name}");
        }
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        if (genieBoss == null)
        {
            genieBoss = FindFirstObjectByType<GenieBossController>();
        }
        
        healthSystem = GetComponent<HealthSystem>();
        if (healthSystem == null)
        {
            healthSystem = gameObject.AddComponent<HealthSystem>();
        }
        healthSystem.maxHealth = maxHealth;
        
        gameObject.tag = "Monster";
    }
    
    private void Update()
    {
        if (isDead) return;
        
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            return;
        }
        
        float distance = Vector3.Distance(transform.position, player.position);
        
        if (distance < detectionRange)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            
            if (rb != null)
            {
                Vector3 targetVelocity = direction * moveSpeed;
                targetVelocity.y = rb.linearVelocity.y;
                rb.linearVelocity = targetVelocity;
                
                if (animator != null)
                {
                    animator.SetBool("isWalking", true);
                    animator.SetFloat("speed", targetVelocity.magnitude);
                    animator.speed = 0.5f;
                }
            }
            
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
        else
        {
            if (rb != null)
            {
                Vector3 velocity = rb.linearVelocity;
                velocity.x = 0;
                velocity.z = 0;
                rb.linearVelocity = velocity;
                
                if (animator != null)
                {
                    animator.SetBool("isWalking", false);
                    animator.SetFloat("speed", 0);
                }
            }
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;
        
        if (collision.gameObject.CompareTag("Player"))
        {
            if (animator != null)
            {
                animator.SetTrigger("attack");
            }
            
            HealthSystem playerHealth = collision.gameObject.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage, transform.position);
            }
            Die();
        }
        else if (collision.gameObject.CompareTag("Projectile"))
        {
            ProjectileController proj = collision.gameObject.GetComponent<ProjectileController>();
            if (proj != null && proj.owner != null && proj.owner.CompareTag("Player"))
            {
                Die();
                Destroy(collision.gameObject);
            }
        }
    }
    
    public void Die()
    {
        if (isDead) return;
        
        isDead = true;
        Debug.Log($"☠️ Monster {gameObject.name} died!");
        
        if (animator != null)
        {
            animator.SetTrigger("die");
            animator.SetBool("isDead", true);
        }
        
        if (genieBoss != null)
        {
            genieBoss.OnMonsterKilled();
        }
        
        Destroy(gameObject, 3f);
    }
}
