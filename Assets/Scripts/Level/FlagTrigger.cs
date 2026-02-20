using UnityEngine;
using GeniesGambit.Core;

namespace GeniesGambit.Level
{
    [RequireComponent(typeof(Collider2D))]
    public class FlagTrigger : MonoBehaviour
    {
        GateController _gateController;

        void Awake()
        {
            _gateController = GetComponent<GateController>();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            
            var iterationMgr = IterationManager.Instance;
            if (iterationMgr == null)
            {
                iterationMgr = FindFirstObjectByType<IterationManager>();
            }

            bool isGhost = other.GetComponent<GeniesGambit.Player.GhostReplay>() != null;
            
            if (isGhost)
            {
                Debug.Log("[Flag] Ghost reached the flag!");
                if (iterationMgr != null)
                {
                    iterationMgr.OnGhostReachedFlag();
                }
                return;
            }
            
            if (!KeyCollectible.HasKey)
            {
                Debug.Log("[Flag] You need the key first!");
                return;
            }
            
            if (_gateController != null)
            {
                _gateController.OpenGate();
            }
            
            if (iterationMgr != null)
            {
                int currentIteration = iterationMgr.CurrentIteration;
                
                if (currentIteration == 1)
                {
                    Debug.Log("[Flag] Hero reached flag in Iteration 1!");
                    iterationMgr.OnHeroReachedFlag();
                }
                else if (currentIteration == 3)
                {
                    Debug.Log("[Flag] Hero reached flag in Iteration 3! Cycle complete!");
                    iterationMgr.OnHeroReachedFlagInIteration3();
                }
                else
                {
                    Debug.LogWarning($"[Flag] Hero reached flag in unexpected iteration: {currentIteration}");
                }
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
