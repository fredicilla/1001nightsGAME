using BossFight;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RewindManager : MonoBehaviour
{
    [Header("UI")]
    public Transform timelineContainer;
    public GameObject turnButtonPrefab;

    private List<Button> turnButtons = new List<Button>();

    private void Start()
    {
        UpdateTimeline();
    }

    public void UpdateTimeline()
    {
        ClearTimeline();

        GameManager gameManager = GameManager.Instance;
        if (gameManager == null) return;

        int currentTurn = Mathf.FloorToInt(gameManager.turnNumber);

        for (int i = 1; i <= currentTurn; i++)
        {
            CreateTurnButton(i);
        }
    }

    private void CreateTurnButton(int turnNumber)
    {
        if (turnButtonPrefab == null || timelineContainer == null) return;

        GameObject buttonObj = Instantiate(turnButtonPrefab, timelineContainer);
        Button button = buttonObj.GetComponent<Button>();

        if (button != null)
        {
            Text buttonText = buttonObj.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = turnNumber.ToString();
            }

            int turn = turnNumber;
            button.onClick.AddListener(() => OnTurnButtonClicked(turn));

            turnButtons.Add(button);
        }
    }

    private void OnTurnButtonClicked(int turnNumber)
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.RewindToTurn(turnNumber);
            UpdateTimeline();
        }
    }

    private void ClearTimeline()
    {
        foreach (Button button in turnButtons)
        {
            if (button != null)
            {
                Destroy(button.gameObject);
            }
        }
        turnButtons.Clear();
    }
}
