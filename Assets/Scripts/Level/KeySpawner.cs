using UnityEngine;

namespace GeniesGambit.Level
{
    public class KeySpawner : MonoBehaviour
    {
        public static KeySpawner Instance { get; private set; }
        public GameObject keyPrefab;
        public Transform spawnPoint;
        private GameObject _currentKey;
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SpawnKey()
        {
            if (_currentKey != null)
                Destroy(_currentKey);
            _currentKey = Instantiate(keyPrefab, spawnPoint.position, Quaternion.identity);
        }

        public void DespawnKey()
        {
            if (_currentKey != null)
                Destroy(_currentKey);
        }
    }
}
