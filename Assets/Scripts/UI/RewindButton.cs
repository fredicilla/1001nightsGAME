using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GeniesGambit.Core;

namespace GeniesGambit.UI
{
    public class RewindButton : MonoBehaviour
    {
        [SerializeField] Button rewindButton;
        [SerializeField] TextMeshProUGUI rewindButtonText;

        float _lastClickTime = -1f;
        const float DOUBLE_CLICK_TIME = 0.5f;

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

            float timeSinceLastClick = Time.time - _lastClickTime;

            if (timeSinceLastClick < DOUBLE_CLICK_TIME && currentIteration > 1)
            {
                int previousIteration = currentIteration - 1;
                IterationManager.Instance.RewindToIteration(previousIteration);
                Debug.Log($"[RewindButton] Double Click - Rewinding to Iteration {previousIteration}");
                _lastClickTime = -1f;
            }
            else
            {
                IterationManager.Instance.RestartCurrentIteration();
                Debug.Log($"[RewindButton] Single Click - Restarting Iteration {currentIteration}");
                _lastClickTime = Time.time;
            }
        }
    }
}
