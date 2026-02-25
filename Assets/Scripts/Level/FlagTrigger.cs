using UnityEngine;
using GeniesGambit.Core;

namespace GeniesGambit.Level
{
    [RequireComponent(typeof(Collider2D))]
    public class FlagTrigger : MonoBehaviour
    {
        GateController _gateController;
        int _triggeredInIteration = -1;  // tracks which iteration already fired — prevents double-trigger per iteration

        void Awake()
        {
            _gateController = GetComponent<GateController>();
        }

        void OnEnable()
        {
            _triggeredInIteration = -1;  // reset when object re-enables
            GameManager.OnStateChanged += HandleStateChange;
        }

        void OnDisable()
        {
            GameManager.OnStateChanged -= HandleStateChange;
        }

        void HandleStateChange(GameState old, GameState newState)
        {
            // Reset every time a new iteration starts so the flag is always triggerable
            if (newState == GameState.HeroTurn || newState == GameState.MonsterTurn)
            {
                _triggeredInIteration = -1;
                Debug.Log("[FlagTrigger] Reset triggeredInIteration on state change");
            }
        }


        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            var iterationMgr = IterationManager.Instance;
            if (iterationMgr == null)
                iterationMgr = FindFirstObjectByType<IterationManager>();

            int currentIteration = iterationMgr?.CurrentIteration ?? 0;
            if (_triggeredInIteration == currentIteration) return;  // already fired this iteration

            bool isGhost = other.GetComponent<GeniesGambit.Player.GhostReplay>() != null;

            if (isGhost)
            {
                iterationMgr?.OnGhostReachedFlag();
                return;
            }

            if (KeyMechanicManager.IsKeyMechanicActive)
            {
                if (!KeyCollectible.HasKey)
                {
                    Debug.Log("[Flag] You need the key first!");
                    return;
                }
            }

            _triggeredInIteration = currentIteration;

            AudioManager.Play(AudioManager.SoundID.Win);
            _gateController?.OpenGate();

            if (iterationMgr != null)
            {
                // Unified callback — IterationManager knows which iteration is active
                iterationMgr.OnHeroReachedGate();
            }
            else
            {
                TurnManager turnManager = FindFirstObjectByType<TurnManager>();
                if (turnManager != null)
                {
                    Debug.Log("[Flag] Treasure unlocked! Calling the Genie...");
                    turnManager.FinishHeroTurn(true);
                }
            }
        }
    }
}

