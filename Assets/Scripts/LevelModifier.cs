using BossFight;
using System.Collections.Generic;
using UnityEngine;

public class LevelModifier : MonoBehaviour
{
    [Header("Wish Objects - Hidden by default")]
    public GameObject agilityPlatforms;
    public GameObject treasureKeyObject;
    public GameObject wisdomQuestionPanel;
    public GameObject flowerSpikesGroup;
    public GameObject wifeGenieObject;

    [Header("Goal Door References")]
    public GameObject normalGoal;
    public GameObject lockedGoal;

    [Header("Active Wishes Tracking")]
    private List<WishType> activeWishes = new List<WishType>();

    [Header("Wisdom State")]
    private bool wisdomActive = false;
    private bool wisdomQuestionShown = false;

    private void Start()
    {
        ResetAll();
    }

    public void ResetAll()
    {
        Debug.Log("🔄 LevelModifier.ResetAll() - Hiding all wish objects...");

        // Hide all wish objects
        if (agilityPlatforms != null) agilityPlatforms.SetActive(false);
        if (treasureKeyObject != null) treasureKeyObject.SetActive(false);
        if (wisdomQuestionPanel != null) wisdomQuestionPanel.SetActive(false);
        if (flowerSpikesGroup != null) flowerSpikesGroup.SetActive(false);
        if (wifeGenieObject != null) wifeGenieObject.SetActive(false);

        // Normal goal is open by default
        if (normalGoal != null) normalGoal.SetActive(true);
        if (lockedGoal != null) lockedGoal.SetActive(false);

        // Clear active wishes
        activeWishes.Clear();

        // Reset wisdom state
        wisdomActive = false;
        wisdomQuestionShown = false;

        Debug.Log("✅ All wish objects hidden!");
    }

    public void ApplyWish(WishType wish)
    {
        Debug.Log($"🧞 LevelModifier.ApplyWish({wish}) called!");

        // Add to active wishes if not already active
        if (!activeWishes.Contains(wish))
        {
            activeWishes.Add(wish);
            Debug.Log($"✅ Added {wish} to active wishes. Total: {activeWishes.Count}");
        }

        GameManager gameManager = GameManager.Instance;

        switch (wish)
        {
            case WishType.Agility:
                Debug.Log("🏃 Activating Agility Platforms...");
                if (agilityPlatforms != null)
                {
                    agilityPlatforms.SetActive(true);
                    Debug.Log("✅ Agility platforms activated!");
                }
                else
                {
                    Debug.LogWarning("⚠️ Agility platforms not assigned in LevelModifier!");
                }
                break;

            case WishType.TreasureKey:
                Debug.Log("🔑 Activating Treasure Key...");
                if (treasureKeyObject != null)
                {
                    treasureKeyObject.SetActive(true);
                    Debug.Log("✅ Treasure key activated!");
                }
                else
                {
                    Debug.LogWarning("⚠️ Treasure key not assigned in LevelModifier!");
                }

                // Lock the goal - requires key
                if (normalGoal != null) normalGoal.SetActive(false);
                if (lockedGoal != null) lockedGoal.SetActive(true);
                if (gameManager != null) gameManager.RequiresKey = true;

                Debug.Log("🔒 Goal is now LOCKED! Key required!");
                break;

            case WishType.Wisdom:
                Debug.Log("📜 Wisdom wish selected - will show question on first move!");
                wisdomActive = true;
                wisdomQuestionShown = false;
                Debug.Log("✅ Wisdom activated! Waiting for player movement...");
                break;

            case WishType.FlowerSpikes:
                Debug.Log("🌸 Activating Flower Spikes...");
                if (flowerSpikesGroup != null)
                {
                    flowerSpikesGroup.SetActive(true);
                    Debug.Log("✅ Flower spikes activated!");
                }
                else
                {
                    Debug.LogWarning("⚠️ Flower spikes not assigned in LevelModifier!");
                }
                break;

            case WishType.Wife:
                Debug.Log("👰 Activating Wife Genie...");
                if (wifeGenieObject != null)
                {
                    wifeGenieObject.SetActive(true);

                    // Start Wife AI if it has one
                    WifeAI wifeAI = wifeGenieObject.GetComponentInChildren<WifeAI>();
                    if (wifeAI != null)
                    {
                        wifeAI.Activate();
                        Debug.Log("✅ Wife AI activated and chasing player!");
                    }
                    else
                    {
                        Debug.Log("✅ Wife genie activated (no AI component)!");
                    }
                }
                else
                {
                    Debug.LogWarning("⚠️ Wife genie not assigned in LevelModifier!");
                }
                break;
        }

        Debug.Log($"📊 Active wishes: {string.Join(", ", activeWishes)}");
    }

    public void ReapplyAllWishes()
    {
        Debug.Log($"🔄 Reapplying all active wishes ({activeWishes.Count})...");

        List<WishType> wishesToReapply = new List<WishType>(activeWishes);
        activeWishes.Clear();

        foreach (WishType wish in wishesToReapply)
        {
            ApplyWish(wish);
        }

        Debug.Log("✅ All wishes reapplied!");
    }

    public bool IsWishActive(WishType wish)
    {
        return activeWishes.Contains(wish);
    }

    public int GetActiveWishCount()
    {
        return activeWishes.Count;
    }

    private void Update()
    {
        // إذا Wisdom نشط ولم يظهر السؤال بعد
        if (wisdomActive && !wisdomQuestionShown)
        {
            // تحقق من حركة اللاعب
            if (CheckPlayerMovement())
            {
                ShowWisdomQuestion();
            }
        }
    }

    private bool CheckPlayerMovement()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return false;

        // تحقق من أي input حركة
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            // إذا اللاعب تحرك
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null && rb.linearVelocity.magnitude > 0.1f)
            {
                return true;
            }
        }

        return false;
    }

    private void ShowWisdomQuestion()
    {
        Debug.Log("🧠 Player moved! Showing Wisdom Question...");

        wisdomQuestionShown = true;

        if (wisdomQuestionPanel != null)
        {
            wisdomQuestionPanel.SetActive(true);

            // جمد اللعبة
            Time.timeScale = 0f;
            Debug.Log("⏸️ Game FROZEN! Answer the question to continue!");
        }
        else
        {
            Debug.LogWarning("⚠️ Wisdom question panel not assigned!");
        }
    }

    public void OnWisdomAnswered()
    {
        Debug.Log("✅ Wisdom question answered! Unfreezing game...");

        // ألغِ التجميد
        Time.timeScale = 1f;

        Debug.Log("▶️ Game RESUMED!");
    }
}
