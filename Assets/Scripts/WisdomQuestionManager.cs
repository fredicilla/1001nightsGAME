using BossFight;
using UnityEngine;
using UnityEngine.UI;

public class WisdomQuestionManager : MonoBehaviour
{
    [Header("UI References")]
    public Button optionAButton;
    public Button optionBButton;
    public Button optionCButton;

    private const int CORRECT_ANSWER = 1; // B (شهرزاد) هي الإجابة الصحيحة دائماً

    private void Awake()
    {
        SetupButtons();
    }

    private void SetupButtons()
    {
        if (optionAButton != null)
        {
            optionAButton.onClick.RemoveAllListeners();
            optionAButton.onClick.AddListener(() => OnAnswerSelected(0));
            Debug.Log("✓ OptionA (علاء الدين) button listener added");
        }

        if (optionBButton != null)
        {
            optionBButton.onClick.RemoveAllListeners();
            optionBButton.onClick.AddListener(() => OnAnswerSelected(1));
            Debug.Log("✓ OptionB (شهرزاد - الإجابة الصحيحة) button listener added");
        }

        if (optionCButton != null)
        {
            optionCButton.onClick.RemoveAllListeners();
            optionCButton.onClick.AddListener(() => OnAnswerSelected(2));
            Debug.Log("✓ OptionC (شهريار) button listener added");
        }

        Debug.Log("✅ Wisdom buttons setup complete! Correct answer: B (شهرزاد)");
    }

    private void OnEnable()
    {
        Debug.Log("🧠 Wisdom Question Panel shown!");
        Debug.Log("📜 السؤال: من هي بطلة ألف ليلة وليلة؟");
        Debug.Log("🎯 الإجابة الصحيحة: B - شهرزاد");
    }

    private void OnAnswerSelected(int selectedAnswer)
    {
        string[] answerNames = { "علاء الدين", "شهرزاد", "شهريار" };
        Debug.Log($"🎯 Answer selected: {(char)('A' + selectedAnswer)} - {answerNames[selectedAnswer]}");

        if (selectedAnswer == CORRECT_ANSWER)
        {
            Debug.Log("✅ CORRECT ANSWER! شهرزاد is correct! Player passes wisdom test!");
            OnCorrectAnswer();
        }
        else
        {
            Debug.Log($"❌ WRONG ANSWER! {answerNames[selectedAnswer]} is incorrect! Player fails!");
            OnWrongAnswer();
        }
    }

    private void OnCorrectAnswer()
    {
        gameObject.SetActive(false);
        Debug.Log("🎉 Wisdom test passed! Continuing game...");

        // ألغِ تجميد اللعبة
        LevelModifier levelModifier = FindFirstObjectByType<LevelModifier>();
        if (levelModifier != null)
        {
            levelModifier.OnWisdomAnswered();
        }
    }

    private void OnWrongAnswer()
    {
        gameObject.SetActive(false);

        // ألغِ تجميد اللعبة
        LevelModifier levelModifier = FindFirstObjectByType<LevelModifier>();
        if (levelModifier != null)
        {
            levelModifier.OnWisdomAnswered();
        }

        Debug.Log("☠️ Wrong answer! Restarting current turn...");

        // إعادة نفس الدور الحالي
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.RestartCurrentTurn();
        }
        else
        {
            Debug.LogError("❌ GameManager.Instance is NULL!");
        }
    }
}
