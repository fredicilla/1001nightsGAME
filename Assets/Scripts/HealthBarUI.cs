using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [Header("References")]
    public HealthSystem targetHealthSystem;
    public Image[] heartImages;
    public Sprite fullHeart;
    public Sprite emptyHeart;
    public TextMeshProUGUI nameText;
    
    [Header("Settings")]
    public bool isPlayerBar = false;
    public Color fullHeartColor = Color.red;
    public Color emptyHeartColor = Color.gray;
    
    private int lastHealth = -1;
    
    private void Start()
    {
        if (targetHealthSystem == null)
        {
            if (isPlayerBar)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    targetHealthSystem = player.GetComponent<HealthSystem>();
                }
            }
            else
            {
                GenieBossController genie = FindObjectOfType<GenieBossController>();
                if (genie != null)
                {
                    targetHealthSystem = genie.GetComponent<HealthSystem>();
                }
            }
        }
        
        UpdateDisplay();
    }
    
    private void Update()
    {
        if (targetHealthSystem != null)
        {
            if (lastHealth != targetHealthSystem.currentHealth)
            {
                UpdateDisplay();
                Debug.Log($"💖 Health changed: {targetHealthSystem.currentHealth}/{targetHealthSystem.maxHealth} for {(isPlayerBar ? "Player" : "Genie")}");
            }
        }
    }
    
    void UpdateDisplay()
    {
        if (targetHealthSystem == null || heartImages == null || heartImages.Length == 0)
        {
            Debug.LogWarning("⚠️ HealthBarUI: Missing targetHealthSystem or heartImages!");
            return;
        }
        
        int health = targetHealthSystem.currentHealth;
        lastHealth = health;
        
        Debug.Log($"🔄 Updating health display: {health} hearts");
        
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] != null)
            {
                if (i < health)
                {
                    if (fullHeart != null)
                        heartImages[i].sprite = fullHeart;
                    heartImages[i].color = fullHeartColor;
                    heartImages[i].enabled = true;
                }
                else
                {
                    if (emptyHeart != null)
                        heartImages[i].sprite = emptyHeart;
                    heartImages[i].color = emptyHeartColor;
                    heartImages[i].enabled = true;
                }
            }
        }
    }
}
