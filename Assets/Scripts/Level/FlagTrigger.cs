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
            
            if (!KeyCollectible.HasKey)
            {
                Debug.Log("[Flag] You need the key first!");
                return;
            }
            
            if (_gateController != null)
            {
                _gateController.OpenGate();
            }
            
            TurnManager turnManager = FindFirstObjectByType<TurnManager>();
            if (turnManager != null)
            {
                Debug.Log("[Flag] Treasure unlocked! Calling the Genie...");
                turnManager.FinishHeroTurn(true);
            }
        }
    }
}
