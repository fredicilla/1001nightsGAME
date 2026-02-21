using UnityEngine;
using GeniesGambit.Core;

namespace GeniesGambit.Level
{
    [RequireComponent(typeof(Collider2D))]
    public class SpikeDamage : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            Debug.Log("[Spikes] Player hit spikes! Respawning...");

            var playerController = other.GetComponent<Player.PlayerController>();
            if (playerController != null)
            {
                playerController.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
                playerController.transform.position = new Vector3(-7.5f, 2.278f, 0);
                playerController.ApplyWeightSlowdown(0f);

                KeyCollectible.ResetKey();
                CoinCollectible.ResetCoins();

                var coinSpawner = FindFirstObjectByType<CoinSpawner>();
                if (coinSpawner != null)
                    coinSpawner.ResetAllCoins();

                var ghost = FindFirstObjectByType<Enemies.ChasingMonster>();
                if (ghost != null)
                    ghost.RespawnGhostPublic();
            }
        }
    }
}
