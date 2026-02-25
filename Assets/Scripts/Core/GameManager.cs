using UnityEngine;
using UnityEngine.Events;
using GeniesGambit.Core;

namespace GeniesGambit.Core
{
    public class GameManager : MonoBehaviour
    {
        // ── Singleton ──────────────────────────────────────────────────────
        public static GameManager Instance { get; private set; }

        // ── State ──────────────────────────────────────────────────────────
        [field: SerializeField]
        public GameState CurrentState { get; private set; } = GameState.HeroTurn;

        // ── Events (subscribe to these from other systems) ─────────────────
        public static event UnityAction<GameState, GameState> OnStateChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetState(GameState newState)
        {
            if (newState == CurrentState) return;
            var old = CurrentState;
            CurrentState = newState;
            OnStateChanged?.Invoke(old, newState);

            Debug.Log($"[GameManager] {old} → {newState}");
        }
    }
}