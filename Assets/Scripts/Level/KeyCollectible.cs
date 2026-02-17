using UnityEngine;

namespace GeniesGambit.Level
{
    public class KeyCollectible : MonoBehaviour
    {
        public static bool HasKey { get; private set; } = false;
        static KeyCollectible _instance;
        Vector3 _startPosition;

        void Awake()
        {
            _instance = this;
            _startPosition = transform.position;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            
            HasKey = true;
            Debug.Log("[Key] Collected! You can now open the treasure!");
            gameObject.SetActive(false);
        }

        public static void ResetKey()
        {
            HasKey = false;
            if (_instance != null)
            {
                _instance.transform.position = _instance._startPosition;
                _instance.gameObject.SetActive(true);
            }
        }
    }
}
