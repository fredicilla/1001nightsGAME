using UnityEngine;
using GeniesGambit.Core;

namespace GeniesGambit.Level
{
    [RequireComponent(typeof(Collider2D))]
    public class SpikeDamage : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            
            Debug.Log("[Spikes] Player hit spikes! Respawning...");
            
            var playerController = other.GetComponent<Player.PlayerController>();
            if (playerController != null)
            {
                playerController.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
                playerController.transform.position = new Vector3(-7, 0, 0);
            }
        }
    }
}
