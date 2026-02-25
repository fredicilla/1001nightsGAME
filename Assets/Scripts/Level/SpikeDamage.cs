using UnityEngine;
using GeniesGambit.Combat;
using GeniesGambit.Core;

namespace GeniesGambit.Level
{
    [RequireComponent(typeof(Collider2D))]
    public class SpikeDamage : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D other)
        {
            // Accept both Player and Enemy tags (player-controlled enemies can also hit spikes)
            if (!other.CompareTag("Player") && !other.CompareTag("Enemy")) return;

            // Ghosts (replaying recordings) should not die from spikes
            if (other.GetComponent<Player.GhostReplay>() != null) return;

            // Freeze the character at the spike — prevents it from flying out of the map
            // NOTE: IterationManager will reset bodyType back to Dynamic on respawn
            var rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }

            AudioManager.Play(AudioManager.SoundID.SpikeDamage);
            // Kill through Health so IterationManager's OnDeath handlers handle the respawn
            var health = other.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(999);
            }
            else
            {
                other.gameObject.SetActive(false);
            }
        }
    }
}
