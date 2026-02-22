using UnityEngine;
using GeniesGambit.Genie;
using GeniesGambit.Level;

namespace GeniesGambit.Core
{
    public class RoundManager : MonoBehaviour
    {
        public static RoundManager Instance { get; private set; }

        [Header("Game Configuration")]
        [SerializeField] int totalRounds = 6;
        [SerializeField] bool enableRound7 = false;

        int _currentRound = 0;

        public int CurrentRound => _currentRound;
        public int TotalRounds => enableRound7 ? totalRounds + 1 : totalRounds;

        public int GetIterationsForCurrentRound()
        {
            return _currentRound <= 2 ? 3 : 5;
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            StartNewRound();
        }

        void StartNewRound()
        {
            _currentRound++;
            Debug.Log($"╔══════════════════════════════════════╗");
            Debug.Log($"║   ROUND {_currentRound} / {TotalRounds}");
            Debug.Log($"╚══════════════════════════════════════╝");

            if (_currentRound > TotalRounds)
            {
                EndGame();
                return;
            }

            if (IterationManager.Instance != null)
            {
                int iterationCount = GetIterationsForCurrentRound();
                IterationManager.Instance.BeginIterationCycle(iterationCount);
            }
            else
            {
                Debug.LogError("[RoundManager] IterationManager not found!");
            }
        }

        public void OnIterationCycleComplete()
        {
            int iterations = GetIterationsForCurrentRound();
            Debug.Log($"[RoundManager] Round {_currentRound} complete! All {iterations} iterations succeeded!");

            if (GenieManager.Instance == null)
            {
                Debug.Log("[RoundManager] No GenieManager found! Moving to next round.");
                StartNewRound();
                return;
            }

            int wishesRemaining = GenieManager.Instance.GetRemainingWishCount();

            if (wishesRemaining == 0)
            {
                Debug.Log("[RoundManager] No wishes remaining! Moving to next round without genie screen.");
                StartNewRound();
                return;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.GenieWishScreen);
            }
        }

        public void OnWishApplied()
        {
            Debug.Log($"[RoundManager] Wish applied. Starting Round {_currentRound + 1}...");
            StartNewRound();
        }

        void EndGame()
        {
            Debug.Log("╔══════════════════════════════════════╗");
            Debug.Log("║   🎉 ALL ROUNDS COMPLETE! 🎉         ║");
            Debug.Log("║   YOU WIN!                           ║");
            Debug.Log("╚══════════════════════════════════════╝");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.LevelComplete);
            }
        }

        public void RestartGame()
        {
            _currentRound = 0;

            if (GenieManager.Instance != null)
            {
                GenieManager.Instance.ResetAllWishes();
            }

            if (KeyMechanicManager.Instance != null)
            {
                KeyMechanicManager.Instance.ResetKeyMechanic();
            }

            StartNewRound();
        }
    }
}
