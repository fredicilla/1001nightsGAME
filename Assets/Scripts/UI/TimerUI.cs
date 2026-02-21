using UnityEngine;
using TMPro;
using GeniesGambit.Core;

namespace GeniesGambit.UI
{
    public class TimerUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI timerText;

        [Header("Color Settings")]
        [SerializeField] Color normalColor = Color.white;
        [SerializeField] Color warningColor = new Color(1f, 0.65f, 0f);
        [SerializeField] Color criticalColor = Color.red;

        [Header("Thresholds")]
        [SerializeField] float warningThreshold = 10f;
        [SerializeField] float criticalThreshold = 5f;

        [Header("Pulse Effect")]
        [SerializeField] bool enablePulse = true;
        [SerializeField] float pulseSpeed = 3f;

        float _pulseTimer = 0f;

        void Update()
        {
            if (IterationManager.Instance == null)
            {
                if (timerText != null)
                    timerText.text = "";
                return;
            }

            IterationTimer timer = IterationManager.Instance.GetComponent<IterationTimer>();
            if (timer == null)
            {
                if (timerText != null)
                    timerText.text = "";
                return;
            }

            if (!timer.IsTimerActive && timer.TimeRemaining <= 0f)
            {
                if (timerText != null)
                    timerText.text = "";
                return;
            }

            float timeRemaining = timer.TimeRemaining;

            if (timerText != null)
            {
                timerText.text = $"Time: {timeRemaining:F1}s";

                if (timeRemaining > warningThreshold)
                {
                    timerText.color = normalColor;
                    timerText.transform.localScale = Vector3.one;
                }
                else if (timeRemaining > criticalThreshold)
                {
                    timerText.color = warningColor;
                    timerText.transform.localScale = Vector3.one;
                }
                else
                {
                    timerText.color = criticalColor;

                    if (enablePulse && timer.IsTimerActive)
                    {
                        _pulseTimer += Time.deltaTime * pulseSpeed;
                        float scale = 1f + Mathf.Sin(_pulseTimer) * 0.1f;
                        timerText.transform.localScale = Vector3.one * scale;
                    }
                    else
                    {
                        timerText.transform.localScale = Vector3.one;
                    }
                }
            }
        }
    }
}
