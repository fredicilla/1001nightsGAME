using UnityEngine;
using UnityEngine.Events;

namespace GeniesGambit.Core
{
    public class IterationTimer : MonoBehaviour
    {
        [Header("Timer Settings")]
        [SerializeField] float timeLimit = 15f;

        public float TimeRemaining { get; private set; }
        public float TimeLimit => timeLimit;
        public bool IsTimerActive { get; private set; }
        public bool HasExpired { get; private set; }

        public UnityEvent OnTimerExpired;

        void Update()
        {
            if (!IsTimerActive) return;

            TimeRemaining -= Time.deltaTime;

            if (TimeRemaining <= 0f && !HasExpired)
            {
                TimeRemaining = 0f;
                HasExpired = true;
                IsTimerActive = false;
                OnTimerExpired?.Invoke();
                Debug.Log("[IterationTimer] Time expired!");
            }
        }

        public void StartTimer()
        {
            TimeRemaining = timeLimit;
            IsTimerActive = true;
            HasExpired = false;
            Debug.Log($"[IterationTimer] Timer started: {timeLimit} seconds");
        }

        public void StopTimer()
        {
            IsTimerActive = false;
            Debug.Log("[IterationTimer] Timer stopped");
        }

        public void ResetTimer()
        {
            TimeRemaining = timeLimit;
            IsTimerActive = false;
            HasExpired = false;
        }

        public float GetNormalizedTime()
        {
            return TimeRemaining / timeLimit;
        }
    }
}
