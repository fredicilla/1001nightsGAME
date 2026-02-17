using UnityEngine;
using GeniesGambit.Core;

namespace GeniesGambit.Level
{
    [RequireComponent(typeof(Collider2D))]
    public class FlagTrigger : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            
            if (!KeyCollectible.HasKey)
            {
                Debug.Log("[Flag] You need the key first!");
                return;
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
