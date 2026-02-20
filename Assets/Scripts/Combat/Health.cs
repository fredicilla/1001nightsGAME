using UnityEngine;

namespace GeniesGambit.Combat
{
    public class Health : MonoBehaviour
    {
        [SerializeField] int maxHealth = 1;

        int _currentHealth;
        bool _isDead = false;

        public event System.Action OnDeath;
        public bool IsDead => _isDead;
        public int CurrentHealth => _currentHealth;
        public int MaxHealth => maxHealth;

        void Start()
        {
            _currentHealth = maxHealth;
        }

        public void TakeDamage(int damage)
        {
            if (_isDead) return;

            _currentHealth -= damage;
            Debug.Log($"[Health] {gameObject.name} took {damage} damage. Health: {_currentHealth}/{maxHealth}");

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        void Die()
        {
            if (_isDead) return;

            _isDead = true;
            Debug.Log($"[Health] {gameObject.name} died!");

            // Disable collider immediately to prevent further hits
            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            // Invoke death event FIRST so handlers can run
            OnDeath?.Invoke();

            // Then deactivate the game object visually (character disappears)
            // Note: This happens after event handlers, so they can still access the object
            gameObject.SetActive(false);
        }

        public void ResetHealth()
        {
            _currentHealth = maxHealth;
            _isDead = false;

            // Re-enable collider (it was disabled on death)
            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = true;
            }

            Debug.Log($"[Health] {gameObject.name} health reset to {_currentHealth}, isDead={_isDead}, collider enabled");
        }

        public void SetInvulnerable(bool invulnerable)
        {
            _isDead = invulnerable;
        }
    }
}
