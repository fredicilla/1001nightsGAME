using BossFight;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    public TextMeshProUGUI turnInfoText;
    public TextMeshProUGUI timerText;

    [Header("Panels")]
    public GameObject geniePanel;
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverMessageText;
    public GameObject victoryPanel;

    [Header("Genie Buttons")]
    public Button agilityButton;
    public Button treasureKeyButton;
    public Button wisdomButton;
    public Button flowerSpikesButton;
    public Button moneyButton;
    public Button wifeButton;

    [Header("Action Buttons")]
    public Button retryButton;
    public Button continueButton;

    private WishType selectedWish = WishType.None;

    private void Start()
    {
        SetupButtons();
        HideAllPanels();
    }

    private void Update()
    {
        UpdateTimer();
    }

    private void SetupButtons()
    {
        // ✅ الأزرار مربوطة في Inspector عبر GenieWishManager
        // لا نحتاج ربط onClick هنا!

        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetry);

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinue);
    }

    public void UpdateTurnInfo(int turnNumber, TurnType turnType)
    {
        if (turnInfoText == null) return;

        string turnName = "";
        string emoji = "";

        switch (turnType)
        {
            case TurnType.HeroTurn:
                turnName = "دور البطل";
                emoji = "🦸";
                break;
            case TurnType.MonsterTurn:
                turnName = "دور الوحش";
                emoji = "👹";
                break;
            case TurnType.SecondMonsterTurn:
                turnName = "دور الوحش الثاني";
                emoji = "👹👹";
                break;
            case TurnType.GenieChoice:
                turnName = "اختيار الأمنية";
                emoji = "🧞";
                break;
        }

        turnInfoText.text = $"{emoji} المرحلة {turnNumber}\n{turnName}";
    }

    private void UpdateTimer()
    {
        if (timerText == null) return;

        GameManager gameManager = GameManager.Instance;
        if (gameManager != null && gameManager.currentTurn != TurnType.GenieChoice)
        {
            float time = gameManager.TimeRemaining;
            int seconds = Mathf.CeilToInt(time);
            timerText.text = $"⏱️ {seconds:00}";

            if (time < 5f)
            {
                timerText.color = Color.red;
            }
            else
            {
                timerText.color = Color.white;
            }
        }
    }

    public void ShowGeniePanel()
    {
        Debug.Log("🧞 UIManager.ShowGeniePanel called!");
        HideAllPanels();
        if (geniePanel != null)
        {
            Debug.Log("🧞 Activating GeniePanel...");
            geniePanel.SetActive(true);

            // Show random wishes
            GenieWishManager genieManager = geniePanel.GetComponent<GenieWishManager>();
            if (genieManager != null)
            {
                Debug.Log("🧞 GenieWishManager found! Calling ShowRandomWishes...");
                genieManager.ShowRandomWishes();
            }
            else
            {
                Debug.LogError("❌ GenieWishManager not found on GeniePanel!");
            }
        }
        else
        {
            Debug.LogError("❌ GeniePanel is null!");
        }
    }

    public void HideGeniePanel()
    {
        Debug.Log("🚫 UIManager.HideGeniePanel() called!");
        if (geniePanel != null)
        {
            geniePanel.SetActive(false);
            Debug.Log("✅ GeniePanel hidden!");
        }
        else
        {
            Debug.LogError("❌ geniePanel is NULL!");
        }
    }

    public void ShowGameOverPanel(string message)
    {
        HideAllPanels();
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (gameOverMessageText != null)
            {
                gameOverMessageText.text = message;
            }
        }
    }

    public void ShowVictoryPanel()
    {
        HideAllPanels();
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
    }

    private void HideAllPanels()
    {
        if (geniePanel != null) geniePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    private void OnRetry()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.RestartLevel();
        }
    }

    private void OnContinue()
    {
        HideAllPanels();
    }
}
