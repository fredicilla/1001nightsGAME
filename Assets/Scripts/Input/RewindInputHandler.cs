using UnityEngine;
using UnityEngine.InputSystem;
using GeniesGambit.Core;

namespace GeniesGambit.Input
{
    public class RewindInputHandler : MonoBehaviour
    {
        float _lastShiftPressTime = -1f;
        const float DOUBLE_CLICK_TIME = 0.5f;

        void Update()
        {
            if (IterationManager.Instance == null) return;

            if (Keyboard.current != null && Keyboard.current.leftShiftKey.wasPressedThisFrame)
            {
                HandleRewindInput();
            }
        }

        void HandleRewindInput()
        {
            int currentIteration = IterationManager.Instance.CurrentIteration;
            if (currentIteration < 1) return;

            float timeSinceLastPress = Time.time - _lastShiftPressTime;

            if (timeSinceLastPress < DOUBLE_CLICK_TIME && currentIteration > 1)
            {
                int previousIteration = currentIteration - 1;
                IterationManager.Instance.RewindToIteration(previousIteration);
                Debug.Log($"[RewindInputHandler] Double Shift - Rewinding to Iteration {previousIteration}");
                _lastShiftPressTime = -1f;
            }
            else
            {
                IterationManager.Instance.RestartCurrentIteration();
                Debug.Log($"[RewindInputHandler] Single Shift - Restarting Iteration {currentIteration}");
                _lastShiftPressTime = Time.time;
            }
        }
    }
}
