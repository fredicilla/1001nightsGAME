using UnityEngine;
using UnityEngine.InputSystem;
using GeniesGambit.Player;

namespace GeniesGambit.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class IterationEnemy : MonoBehaviour
    {
        [Header("Visual Settings")]
        [SerializeField] Color enemyColor = new Color(1f, 0.3f, 0.3f, 1f);

        Rigidbody2D _rb;
        SpriteRenderer _spriteRenderer;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void Start()
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = enemyColor;
            }

            gameObject.tag = "Enemy";
            gameObject.name = "IterationEnemy (Player Controlled)";
        }

        public void EnablePlayerControl(bool enable)
        {
            var playerController = GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = enable;
            }

            var playerInput = GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.enabled = enable;
            }
        }
    }
}
