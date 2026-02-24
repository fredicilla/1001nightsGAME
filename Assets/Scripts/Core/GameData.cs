using UnityEngine;

namespace GeniesGambit.Core
{
    /// <summary>
    /// Persists across scene loads to carry 2D game results into the boss fight.
    /// </summary>
    public class GameData : MonoBehaviour
    {
        public static GameData Instance { get; private set; }

        // Data carried from 2D → Boss Fight
        public int CoinsCollected { get; set; }
        public int WishesGranted { get; set; }
        public int RoundsCompleted { get; set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
