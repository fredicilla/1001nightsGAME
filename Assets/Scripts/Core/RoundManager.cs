using UnityEngine;
using GeniesGambit.Genie;
using GeniesGambit.Level;

namespace GeniesGambit.Core
{
    /// <summary>
    /// Thin bootstrap — kicks off the 7-iteration game loop managed by IterationManager.
    /// Kept alive so existing references (GenieManager, QuizManager, etc.) that check
    /// RoundManager.Instance don't null-ref while they're being migrated.
    /// </summary>
    public class RoundManager : MonoBehaviour
    {
        public static RoundManager Instance { get; private set; }

        // Legacy property — QuizManager still reads this.
        // Returns IterationManager.CurrentIteration so callers get a useful number.
        public int CurrentRound => IterationManager.Instance != null
            ? IterationManager.Instance.CurrentIteration
            : 0;

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
            if (IterationManager.Instance != null)
            {
                // Reapply persistent wish effects so they carry over on scene reload
                if (GenieManager.Instance != null)
                    GenieManager.Instance.ReapplyPersistentWishEffects();

                IterationManager.Instance.BeginGame();
            }
            else
            {
                Debug.LogError("[RoundManager] IterationManager not found!");
            }
        }

        /// <summary>Called by GenieManager after a wish is chosen.</summary>
        public void OnWishApplied()
        {
            Debug.Log("[RoundManager] Wish applied — forwarding to IterationManager.");

            // Reapply all wish effects so they persist into the next iteration
            if (GenieManager.Instance != null)
                GenieManager.Instance.ReapplyPersistentWishEffects();

            if (IterationManager.Instance != null)
                IterationManager.Instance.OnWishApplied();
        }

        // Legacy stubs kept so nothing breaks at compile time
        public void OnIterationCycleComplete() { }

        public void RestartGame()
        {
            if (IterationManager.Instance != null)
                IterationManager.Instance.RestartGame();
        }
    }
}
