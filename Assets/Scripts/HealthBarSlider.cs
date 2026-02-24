using UnityEngine;
using UnityEngine.UI;

public class HealthBarSlider : MonoBehaviour
{
    [Header("References")]
    public HealthSystem targetHealthSystem;
    public RectTransform fillBar;
    public Image fillImage;
    
    [Header("Settings")]
    public bool isPlayerBar = false;
    public Color fillColor = Color.green;
    public bool smoothTransition = true;
    public float transitionSpeed = 10f;
    
    private float maxWidth;
    private float targetWidth;
    private float currentWidth;
    
    private void Start()
    {
        FindAndSetupTarget();
        SetupFillBar();
    }
    
    private void FindAndSetupTarget()
    {
        if (fillBar == null)
        {
            if (fillImage != null)
            {
                fillBar = fillImage.GetComponent<RectTransform>();
            }
        }
        
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
                GenieBossController genie = FindFirstObjectByType<GenieBossController>();
                if (genie != null)
                {
                    targetHealthSystem = genie.GetComponent<HealthSystem>();
                }
            }
        }
        
        if (targetHealthSystem != null)
        {
            targetHealthSystem.onHealthChanged.AddListener(OnHealthChanged);
            Debug.Log($"✅ HealthBarSlider connected to {(isPlayerBar ? "Player" : "Genie")}");
        }
    }
    
    private void SetupFillBar()
    {
        if (fillBar != null)
        {
            maxWidth = fillBar.sizeDelta.x;
            currentWidth = maxWidth;
            targetWidth = maxWidth;
            
            if (fillImage != null)
            {
                fillImage.color = fillColor;
            }
            
            Debug.Log($"🎨 HealthBar setup - {(isPlayerBar ? "Player" : "Genie")}: MaxWidth={maxWidth}");
        }
        else
        {
            Debug.LogError($"❌ fillBar is null for {(isPlayerBar ? "Player" : "Genie")}!");
        }
        
        if (targetHealthSystem != null)
        {
            OnHealthChanged(targetHealthSystem.currentHealth, targetHealthSystem.maxHealth);
        }
    }
    
    private void OnHealthChanged(int currentHealth, int maxHealth)
    {
        if (maxHealth <= 0) return;
        
        float healthPercent = Mathf.Clamp01((float)currentHealth / (float)maxHealth);
        targetWidth = maxWidth * healthPercent;
        
        Debug.Log($"💊 {(isPlayerBar ? "Player" : "Genie")} Health: {currentHealth}/{maxHealth} = {healthPercent * 100:F0}% → Width: {targetWidth:F1}px");
        
        if (!smoothTransition && fillBar != null)
        {
            currentWidth = targetWidth;
            fillBar.sizeDelta = new Vector2(targetWidth, fillBar.sizeDelta.y);
        }
    }
    
    private void Update()
    {
        if (fillBar != null && smoothTransition)
        {
            currentWidth = Mathf.Lerp(currentWidth, targetWidth, transitionSpeed * Time.deltaTime);
            fillBar.sizeDelta = new Vector2(currentWidth, fillBar.sizeDelta.y);
        }
    }
    
    private void OnDestroy()
    {
        if (targetHealthSystem != null)
        {
            targetHealthSystem.onHealthChanged.RemoveListener(OnHealthChanged);
        }
    }
}
