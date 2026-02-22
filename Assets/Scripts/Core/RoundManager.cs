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
        bool _playingBonusRound = false;   // extra iteration after the last wish

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

            AudioManager.Play(AudioManager.SoundID.NewRound);
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

            // If this was the bonus round that plays after the last wish, end the game now.
            if (_playingBonusRound)
            {
                _playingBonusRound = false;
                Debug.Log("[RoundManager] Bonus round complete. Game over — you win!");
                EndGame();
                return;
            }

            if (GenieManager.Instance == null)
            {
                // No genie system — transition through GenieWishScreen anyway so
                // state-change listeners (input, coins, FlagTrigger) fire correctly,
                // then immediately start the next round.
                if (GameManager.Instance != null)
                    GameManager.Instance.SetState(GameState.GenieWishScreen);
                StartNewRound();
                return;
            }

            // ALWAYS transition through GenieWishScreen so every registered listener
            // (PlayerController input toggle, CoinSpawner, FlagTrigger reset, etc.)
            // gets the correct old→new state pair and resets properly.  If there are
            // no wishes left, GenieManager.BeginWishSelection will call OnWishApplied
            // directly (skipping the panel) which starts the next round from
            // GenieWishScreen state — guaranteeing the HeroTurn event fires cleanly.
            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.GenieWishScreen);
        }

        public void OnWishApplied()
        {
            Debug.Log($"[RoundManager] Wish applied. Starting Round {_currentRound + 1}...");

            // If the NEXT call to StartNewRound would exceed TotalRounds, run a bonus
            // iteration instead so the player gets to experience their last wish.
            if (_currentRound >= TotalRounds)
            {
                Debug.Log("[RoundManager] Last round wish applied — starting BONUS iteration!");
                _playingBonusRound = true;

                if (IterationManager.Instance != null)
                {
                    int iterationCount = GetIterationsForCurrentRound();
                    IterationManager.Instance.BeginIterationCycle(iterationCount);
                }
                return;
            }

            StartNewRound();
        }

        [Header("Victory Screen")]
        [SerializeField] GameObject victoryScreen;

        void EndGame()
        {
            Debug.Log("╔══════════════════════════════════════╗");
            Debug.Log("║   ALL ROUNDS COMPLETE! YOU WIN!      ║");
            Debug.Log("╚══════════════════════════════════════╝");

            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.LevelComplete);

            AudioManager.Play(AudioManager.SoundID.GameWin);
            if (victoryScreen != null)
            {
                victoryScreen.SetActive(true);
                Debug.Log("[RoundManager] Victory screen shown.");
            }
            else
            {
                // No victory screen wired in inspector — restart after 3 seconds
                Debug.Log("[RoundManager] No victory screen assigned. Auto-restarting in 3s...");
                Invoke(nameof(RestartGame), 3f);
            }
        }

        public void RestartGame()
        {
            _currentRound = 0;
            _playingBonusRound = false;

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
