using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GeniesGambit.Core;

namespace GeniesGambit.Level
{
    public class CoinSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] GameObject coinPrefab;
        [SerializeField] int coinsPerBatch = 8;
        [SerializeField] int targetCoinsToCollect = 8;
        [SerializeField] Vector2 spawnAreaMin = new Vector2(-7f, 8f);
        [SerializeField] Vector2 spawnAreaMax = new Vector2(6f, 12f);
        [SerializeField] float respawnDelay = 0.5f;
        [SerializeField] float spawnInterval = 0.3f;
        [SerializeField] float batchRespawnDelay = 2f;
        
        List<GameObject> _spawnedCoins = new List<GameObject>();
        bool _isActive = false;

        void Start()
        {
            Debug.Log($"[CoinSpawner] Start called. Current coins collected: {CoinCollectible.CoinsCollected}");
            CoinCollectible.ResetCoins();
            Debug.Log("[CoinSpawner] Coins reset to 0, initiating coin spawning");
            _isActive = true;
            StartCoroutine(SpawnCoinsUntilTarget());
        }

        void OnEnable()
        {
            GameManager.OnStateChanged += HandleStateChange;
        }

        void OnDisable()
        {
            GameManager.OnStateChanged -= HandleStateChange;
        }

        void HandleStateChange(GameState oldState, GameState newState)
        {
            if (newState == GameState.GenieWishScreen)
            {
                Debug.Log("[CoinSpawner] Wish screen reached - pausing spawner and cleaning up coins");
                _isActive = false;
                StopAllCoroutines();
                
                foreach (var coin in _spawnedCoins)
                {
                    if (coin != null)
                        Destroy(coin);
                }
                _spawnedCoins.Clear();
            }
            else if (newState == GameState.HeroTurn && oldState == GameState.GenieWishScreen)
            {
                Debug.Log("[CoinSpawner] Resuming coin spawning for new round");
                CoinCollectible.ResetCoins();
                _isActive = true;
                StartCoroutine(SpawnCoinsUntilTarget());
            }
        }

        IEnumerator SpawnCoinsUntilTarget()
        {
            Debug.Log($"[CoinSpawner] Starting spawn loop until {targetCoinsToCollect} coins collected");
            
            yield return new WaitForSeconds(0.1f);
            
            while (_isActive && CoinCollectible.CoinsCollected < targetCoinsToCollect)
            {
                yield return StartCoroutine(SpawnBatch());
                
                if (_isActive && CoinCollectible.CoinsCollected < targetCoinsToCollect)
                {
                    Debug.Log($"[CoinSpawner] Waiting {batchRespawnDelay}s before spawning next batch. Collected: {CoinCollectible.CoinsCollected}/{targetCoinsToCollect}");
                    yield return new WaitForSeconds(batchRespawnDelay);
                }
            }
            
            if (_isActive)
            {
                Debug.Log($"[CoinSpawner] Target reached! Coins collected: {CoinCollectible.CoinsCollected}/{targetCoinsToCollect}");
            }
        }

        IEnumerator SpawnBatch()
        {
            Debug.Log($"[CoinSpawner] Spawning batch of {coinsPerBatch} coins");
            
            for (int i = 0; i < coinsPerBatch; i++)
            {
                if (!_isActive)
                {
                    Debug.Log("[CoinSpawner] Batch spawning interrupted - spawner deactivated");
                    yield break;
                }
                
                Vector3 randomPos = new Vector3(
                    Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                    Random.Range(spawnAreaMin.y, spawnAreaMax.y),
                    0f
                );
                
                GameObject coin = Instantiate(coinPrefab, randomPos, Quaternion.identity, transform);
                _spawnedCoins.Add(coin);
                
                Debug.Log($"[CoinSpawner] Spawned coin {i + 1}/{coinsPerBatch} at {randomPos}");
                
                yield return new WaitForSeconds(spawnInterval);
            }
            
            Debug.Log($"[CoinSpawner] Batch complete. Waiting for coins to be collected or fall off...");
        }

        public void ResetAllCoins()
        {
            Debug.Log("[CoinSpawner] Resetting all coins");
            StopAllCoroutines();
            CoinCollectible.ResetCoins();
            
            foreach (var coin in _spawnedCoins)
            {
                if (coin != null)
                    Destroy(coin);
            }
            
            _spawnedCoins.Clear();
            
            if (gameObject.activeInHierarchy && _isActive)
                StartCoroutine(DelayedSpawn());
        }

        IEnumerator DelayedSpawn()
        {
            Debug.Log($"[CoinSpawner] Delaying respawn for {respawnDelay}s after reset");
            yield return new WaitForSeconds(respawnDelay);
            StartCoroutine(SpawnCoinsUntilTarget());
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3(
                (spawnAreaMin.x + spawnAreaMax.x) / 2f,
                (spawnAreaMin.y + spawnAreaMax.y) / 2f,
                0f
            );
            Vector3 size = new Vector3(
                spawnAreaMax.x - spawnAreaMin.x,
                spawnAreaMax.y - spawnAreaMin.y,
                0.1f
            );
            Gizmos.DrawWireCube(center, size);
        }
    }
}
