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
            OnDeath?.Invoke();
        }

        public void ResetHealth()
        {
            _currentHealth = maxHealth;
            _isDead = false;
        }

        public void SetInvulnerable(bool invulnerable)
        {
            _isDead = invulnerable;
        }
    }
}
