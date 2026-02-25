using UnityEngine;
using UnityEngine.InputSystem;
using GeniesGambit.Core;

namespace GeniesGambit.Input
{
    public class RewindInputHandler : MonoBehaviour
    {
        float _lastRewindTime = -10f;
        const float REWIND_COOLDOWN = 0.2f;

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
            if (Time.time - _lastRewindTime < REWIND_COOLDOWN)
                return;

            int currentIteration = IterationManager.Instance.CurrentIteration;
            if (currentIteration < 1) return;

            if (currentIteration <= 1)
            {
                IterationManager.Instance.ResetIterations();
                Debug.Log("[RewindInputHandler] Shift - Resetting entire cycle from Iteration 1");
            }
            else
            {
                int previousIteration = currentIteration - 1;
                IterationManager.Instance.RewindToIteration(previousIteration);
                Debug.Log($"[RewindInputHandler] Shift - Rewinding to Iteration {previousIteration}");
            }

            _lastRewindTime = Time.time;
        }
    }
}
