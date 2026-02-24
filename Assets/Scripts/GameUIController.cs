using BossFight;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    [Header("Timer UI")]
    public Image timerBar;
    public Text timerText;

    [Header("Player Indicator")]
    public CanvasGroup playerIndicator;
    public RectTransform playerIndicatorTransform;

    private GameManager gameManager;
    private Camera mainCamera;

    private void Start()
    {
        gameManager = GameManager.Instance;
        mainCamera = Camera.main;

        if (playerIndicator != null)
        {
            playerIndicator.alpha = 0;
        }
    }

    private void Update()
    {
        if (gameManager == null) return;

        UpdateTimer();
        UpdatePlayerIndicator();
    }

    private void UpdateTimer()
    {
        if (timerBar == null || timerText == null) return;

        float timeRemaining = gameManager.TimeRemaining;
        float maxTime = gameManager.turnDuration;

        float fillAmount = timeRemaining / maxTime;
        timerBar.fillAmount = fillAmount;

        timerText.text = Mathf.Ceil(timeRemaining).ToString();

        if (fillAmount > 0.5f)
        {
            timerBar.color = Color.green;
        }
        else if (fillAmount > 0.25f)
        {
            timerBar.color = Color.yellow;
        }
        else
        {
            timerBar.color = Color.red;
        }
    }

    private void UpdatePlayerIndicator()
    {
        if (playerIndicator == null || playerIndicatorTransform == null) return;

        bool shouldShow = gameManager.IsWaitingForInput;

        playerIndicator.alpha = Mathf.Lerp(playerIndicator.alpha, shouldShow ? 1f : 0f, Time.deltaTime * 5f);

        if (shouldShow && gameManager.CurrentPlayer != null)
        {
            Vector3 playerWorldPos = gameManager.CurrentPlayer.transform.position + Vector3.up * 3f;
            Vector3 screenPos = mainCamera.WorldToScreenPoint(playerWorldPos);
            playerIndicatorTransform.position = screenPos;

            float bounce = Mathf.Sin(Time.time * 3f) * 10f;
            playerIndicatorTransform.anchoredPosition = new Vector2(
                playerIndicatorTransform.anchoredPosition.x,
                playerIndicatorTransform.anchoredPosition.y + bounce * Time.deltaTime
            );
        }
    }
}
