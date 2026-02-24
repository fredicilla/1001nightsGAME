using UnityEngine;
using GeniesGambit.Core;

namespace GeniesGambit.Level
{
    public class KeyCollectible : MonoBehaviour
    {
        [Header("Bob Settings")]
        public float bobHeight = 0.12f;
        public float bobSpeed = 2f;

        [Header("Glow Settings")]
        public float pulseSpeed = 2f;
        public float minBrightness = 4f;
        public float maxBrightness = 4.1f;

        public static bool HasKey { get; private set; } = false;
        static KeyCollectible _instance;
        Vector3 _startPosition;
        SpriteRenderer _sr;
        Color _originalColor;

        void Awake()
        {
            _instance = this;
            _startPosition = transform.position;
            _sr = GetComponent<SpriteRenderer>();
            if (_sr != null)
                _originalColor = _sr.color;

            if (!KeyMechanicManager.IsKeyMechanicActive)
            {
                gameObject.SetActive(false);
            }
        }

        void Update()
        {
            // Bob up and down
            float newY = _startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            // Glow: pulse the sprite brightness
            if (_sr != null)
            {
                float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
                float brightness = Mathf.Lerp(minBrightness, maxBrightness, t);
                _sr.color = _originalColor * brightness;
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            HasKey = true;
            AudioManager.Play(AudioManager.SoundID.KeyCollect);
            Debug.Log("[Key] Collected! You can now open the treasure!");
            gameObject.SetActive(false);
        }

        public static void ResetKey()
        {
            HasKey = false;
            if (_instance != null)
            {
                _instance.transform.position = _instance._startPosition;

                if (KeyMechanicManager.IsKeyMechanicActive)
                {
                    _instance.gameObject.SetActive(true);
                }
                else
                {
                    _instance.gameObject.SetActive(false);
                }
            }
        }
    }
}

