using UnityEngine;
using UnityEngine.UI;

public class SimpleHealthBar : MonoBehaviour
{
    [Header("Target")]
    public HealthSystem healthSystem;
    public bool autoFindPlayer = false;
    public bool autoFindGenie = false;
    
    [Header("UI References")]
    public Image backgroundImage;
    public Image fillImage;
    
    [Header("Settings")]
    public Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    public Color fillColor = Color.green;
    public bool fillFromLeft = true;
    
    private RectTransform fillRect;
    private float maxBarWidth;
    
    private void Start()
    {
        if (healthSystem == null)
        {
            if (autoFindPlayer)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    healthSystem = player.GetComponent<HealthSystem>();
                }
            }
            else if (autoFindGenie)
            {
                GenieBossController genie = FindFirstObjectByType<GenieBossController>();
                if (genie != null)
                {
                    healthSystem = genie.GetComponent<HealthSystem>();
                }
            }
        }
        
        if (fillImage != null)
        {
            fillRect = fillImage.GetComponent<RectTransform>();
            fillImage.color = fillColor;
            
            if (fillRect != null)
            {
                maxBarWidth = fillRect.rect.width;
            }
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
        }
        
        if (healthSystem != null)
        {
            healthSystem.onHealthChanged.AddListener(UpdateBar);
            UpdateBar(healthSystem.currentHealth, healthSystem.maxHealth);
        }
    }
    
    private void UpdateBar(int currentHealth, int maxHealth)
    {
        if (fillRect == null || maxHealth <= 0) return;
        
        float healthPercent = Mathf.Clamp01((float)currentHealth / (float)maxHealth);
        float targetWidth = maxBarWidth * healthPercent;
        
        Vector2 sizeDelta = fillRect.sizeDelta;
        sizeDelta.x = targetWidth;
        fillRect.sizeDelta = sizeDelta;
        
        if (!fillFromLeft)
        {
            Vector2 anchoredPos = fillRect.anchoredPosition;
            anchoredPos.x = -(maxBarWidth - targetWidth);
            fillRect.anchoredPosition = anchoredPos;
        }
    }
    
    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.onHealthChanged.RemoveListener(UpdateBar);
        }
    }
}
