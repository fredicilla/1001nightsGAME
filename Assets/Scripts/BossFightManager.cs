using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using GeniesGambit.Core;

public class BossFightManager : MonoBehaviour
{
    public static BossFightManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject victoryPanel;
    public GameObject gameOverPanel;
    public Button restartButton;
    public Button retryButton;
    public TextMeshProUGUI victoryText;
    public TextMeshProUGUI gameOverText;

    [Header("State")]
    public bool battleEnded = false;

    [Header("Auto Restart Settings")]
    public bool autoRestartOnPlayerDeath = true;
    public float restartDelay = 2f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        HideAllPanels();
        SetupButtons();
    }

    void HideAllPanels()
    {
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    void SetupButtons()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartBattle);

        if (retryButton != null)
            retryButton.onClick.AddListener(RestartBattle);
    }

    public void OnGenieDefeated()
    {
        if (battleEnded) return;

        battleEnded = true;
        Debug.Log("🎉 Victory! Genie Defeated!");

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);

            if (victoryText != null)
                victoryText.text = "YOU WIN!";
        }

        Time.timeScale = 0.5f;
        Invoke(nameof(ResetTimeScale), 2f);

        // Return to 2D game (Level1) after victory
        Invoke(nameof(LoadLevel1), 4f);
    }

    void LoadLevel1()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SampleScene");
    }

    public void OnPlayerDeath()
    {
        if (battleEnded) return;

        battleEnded = true;
        Debug.Log("💀 Game Over! Player Died!");

        if (autoRestartOnPlayerDeath)
        {
            Debug.Log($"⏰ Auto-restarting in {restartDelay} seconds...");
            Invoke(nameof(RestartBattle), restartDelay);
        }
        else
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);

                if (gameOverText != null)
                    gameOverText.text = "هزيمة!\nحاول مرة أخرى";
            }
        }
    }

    void ResetTimeScale()
    {
        Time.timeScale = 1f;
    }

    public void RestartBattle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
