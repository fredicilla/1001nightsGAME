using BossFight;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class HealthChangedEvent : UnityEvent<int, int> { }

public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;

    [Header("Events")]
    public HealthChangedEvent onHealthChanged = new HealthChangedEvent();

    private int _currentHealth;
    public int currentHealth
    {
        get => _currentHealth;
        private set
        {
            if (_currentHealth != value)
            {
                _currentHealth = value;
                onHealthChanged?.Invoke(_currentHealth, maxHealth);
            }
        }
    }

    [Header("Spawn Protection")]
    public float spawnProtectionDuration = 0.5f;
    private float spawnTime;
    public bool hasSpawnProtection = true;

    [Header("Death Settings")]
    public float deathAnimationDuration = 3f;

    private PlayerAnimationController animationController;
    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        spawnTime = Time.time;
        animationController = GetComponent<PlayerAnimationController>();
        Debug.Log($"{gameObject.name} HealthSystem: {currentHealth}/{maxHealth} HP (Spawn protection: {spawnProtectionDuration}s)");
    }

    private void Update()
    {
        if (hasSpawnProtection && Time.time - spawnTime > spawnProtectionDuration)
        {
            hasSpawnProtection = false;
            Debug.Log($"🛡️ {gameObject.name} spawn protection ended!");
        }
    }

    public void TakeDamage(int damage, Vector3 damageSourcePosition)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"💔 {gameObject.name} took {damage} damage! HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Vector3 damageDirection = (transform.position - damageSourcePosition).normalized;
            Die(damageDirection);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasSpawnProtection || isDead) return;

        string myTag = gameObject.tag;
        string otherTag = collision.gameObject.tag;

        Vector3 contactPoint = collision.contacts.Length > 0
            ? collision.contacts[0].point
            : collision.transform.position;

        if (myTag == "Player" && otherTag == "Monster")
        {
            TakeDamage(1, contactPoint);
        }
        else if (myTag == "Enemy" && otherTag == "Monster")
        {
            return;
        }

        if (otherTag == "Projectile")
        {
            ProjectileController proj = collision.gameObject.GetComponent<ProjectileController>();

            if (proj != null && proj.owner != null)
            {
                if (myTag == "Player" && proj.owner.CompareTag("Enemy"))
                {
                    TakeDamage(1, contactPoint);
                    Destroy(collision.gameObject);
                }
                else if (myTag == "Enemy" && proj.owner.CompareTag("Player"))
                {
                    TakeDamage(1, contactPoint);
                    Destroy(collision.gameObject);
                }
            }
            else
            {
                Destroy(collision.gameObject);
            }
        }
        else if (otherTag == "EnemyProjectile")
        {
            if (myTag == "Player")
            {
                TakeDamage(1, contactPoint);
                Destroy(collision.gameObject);
            }
            else
            {
                Destroy(collision.gameObject);
            }
        }
    }

    public void Die(Vector3 damageDirection)
    {
        if (isDead) return;

        isDead = true;
        Debug.Log($"☠️ {gameObject.name} died!");

        GenieBossController genieBoss = GetComponent<GenieBossController>();
        if (genieBoss != null)
        {
            Debug.Log("🎯 HealthSystem detected GenieBossController - delegating to GenieBossController.Die()");
            return;
        }

        if (animationController != null)
        {
            animationController.TriggerDeath(damageDirection);
        }

        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.IsActive = false;
        }

        Destroy(gameObject, deathAnimationDuration);

        BossFightManager bfm = FindFirstObjectByType<BossFightManager>();
        if (bfm != null)
        {
            if (gameObject.CompareTag("Player"))
            {
                bfm.OnPlayerDeath();
            }
            else if (gameObject.CompareTag("Enemy"))
            {
                Debug.Log("☠️ Enemy defeated - calling OnGenieDefeated!");
                bfm.OnGenieDefeated();
            }
        }
    }
}
