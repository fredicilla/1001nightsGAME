using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace GeniesGambit.UI
{
    /// <summary>
    /// Add this to the EventSystem GameObject.
    /// After any UI button click the selected object is cleared and all PlayerInput
    /// components re-activate so keyboard / gamepad input returns to the player.
    /// </summary>
    public class DeselectOnClick : MonoBehaviour
    {
        void Update()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
            {
                EventSystem.current?.SetSelectedGameObject(null);

                // Re-activate all PlayerInput components so they reclaim device focus
                foreach (var pi in FindObjectsByType<PlayerInput>(FindObjectsSortMode.None))
                {
                    if (pi.enabled && pi.gameObject.activeInHierarchy)
                        pi.ActivateInput();
                }
            }
        }
    }
}
