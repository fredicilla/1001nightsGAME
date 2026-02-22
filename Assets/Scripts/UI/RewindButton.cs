using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using GeniesGambit.Core;

namespace GeniesGambit.UI
{
    public class RewindButton : MonoBehaviour
    {
        [SerializeField] Button rewindButton;
        [SerializeField] TextMeshProUGUI rewindButtonText;

        void Start()
        {
            if (rewindButton != null)
            {
                rewindButton.onClick.AddListener(OnRewindClicked);
            }
        }

        void Update()
        {
            if (IterationManager.Instance == null || rewindButton == null)
            {
                if (rewindButton != null)
                {
                    rewindButton.gameObject.SetActive(false);
                }
                return;
            }

            // Keyboard shortcut: Left Shift triggers rewind (same logic as button click)
            if (Keyboard.current != null && Keyboard.current.leftShiftKey.wasPressedThisFrame)
            {
                OnRewindClicked();
            }

            int currentIteration = IterationManager.Instance.CurrentIteration;
            bool canRewind = currentIteration >= 1;

            rewindButton.gameObject.SetActive(canRewind);

            if (canRewind && rewindButtonText != null)
            {
                rewindButtonText.text = $"REWIND\n(Shift)";
            }
        }

        void OnRewindClicked()
        {
            if (IterationManager.Instance == null) return;

            int currentIteration = IterationManager.Instance.CurrentIteration;
            if (currentIteration < 1) return;

            if (currentIteration <= 1)
            {
                // At iteration 1 — can't go back further, just restart from scratch
                IterationManager.Instance.ResetIterations();
                Debug.Log("[RewindButton] At Iteration 1 — resetting entire cycle");
            }
            else
            {
                // Rewind to the previous iteration
                int previousIteration = currentIteration - 1;
                IterationManager.Instance.RewindToIteration(previousIteration);
                Debug.Log($"[RewindButton] Rewinding from Iteration {currentIteration} to Iteration {previousIteration}");
            }
        }
    }
}
