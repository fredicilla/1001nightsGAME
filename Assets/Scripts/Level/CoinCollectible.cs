using UnityEngine;
using GeniesGambit.Core;

namespace GeniesGambit.Level
{
    public class CoinCollectible : MonoBehaviour
    {
        [Header("Coin Settings")]
        [SerializeField] float slowdownPerCoin = 0.5f;

        public static int CoinsCollected { get; private set; } = 0;
        public static float TotalSlowdown { get; private set; } = 0f;

        bool _collected = false;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected || !other.CompareTag("Player")) return;

            _collected = true;
            CoinsCollected++;
            TotalSlowdown += slowdownPerCoin;
            AudioManager.Play(AudioManager.SoundID.CoinCollect);

            Debug.Log($"[Coin] Collected! Total: {CoinsCollected}, Slowdown: {TotalSlowdown}");

            var playerController = other.GetComponent<Player.PlayerController>();
            if (playerController != null)
                playerController.ApplyWeightSlowdown(TotalSlowdown);

            gameObject.SetActive(false);
        }

        public static void ResetCoins()
        {
            CoinsCollected = 0;
            TotalSlowdown = 0f;
            Debug.Log("[Coin] Reset all coins!");
        }
    }
}
