using UnityEngine;
using GeniesGambit.Core;

namespace GeniesGambit.Core
{
    public class TurnManager : MonoBehaviour
    {
        [Header("Timing")]
        [SerializeField] float heroTurnDuration = 0f; // 0 = no time limit
        float _timer;

        void Update()
        {
            if (GameManager.Instance.CurrentState != GameState.HeroTurn) return;
            if (heroTurnDuration <= 0f) return;
            _timer -= Time.deltaTime;
            if (_timer <= 0f) FinishHeroTurn(false);
        }

        // Call this when Hero reaches the flag/treasure
        public void FinishHeroTurn(bool success)
        {
            GameManager.Instance.SetState(
                success ? GameState.GenieWishScreen : GameState.GameOver);
        }

        // Call this when Hero is killed
        public void HeroKilled() =>
            GameManager.Instance.SetState(GameState.GameOver);
    }
}