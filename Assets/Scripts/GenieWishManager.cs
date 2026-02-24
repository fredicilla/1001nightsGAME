using BossFight;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GenieWishManager : MonoBehaviour
{
    [Header("Wish Buttons")]
    public Button athleticButton;
    public Button wifeButton;
    public Button wisdomButton;
    public Button moneyButton;
    public Button flowersButton;

    [Header("Wish Images")]
    public Image athleticImage;
    public Image wifeImage;
    public Image wisdomImage;
    public Image moneyImage;
    public Image flowersImage;

    private List<WishType> availableWishes = new List<WishType>();
    private List<WishType> selectedWishes = new List<WishType>();
    private Dictionary<WishType, Button> wishButtons = new Dictionary<WishType, Button>();
    private Dictionary<WishType, Image> wishImages = new Dictionary<WishType, Image>();
    private bool isSelectingWish = false;

    private void Awake()
    {
        Debug.Log("🧞 GenieWishManager.Awake called!");
        InitializeWishes();
        SetupButtons();
        HideAllWishes();
    }

    private void HideAllWishes()
    {
        foreach (var kvp in wishButtons)
        {
            if (kvp.Value != null)
            {
                kvp.Value.gameObject.SetActive(false);
            }
        }
        Debug.Log("🙈 All wish buttons hidden initially");
    }

    private void InitializeWishes()
    {
        Debug.Log("🧞 GenieWishManager.InitializeWishes called!");

        // جميع الأمنيات الـ 5
        availableWishes = new List<WishType>
        {
            WishType.Agility,      // Athletic
            WishType.Wife,         // Wife
            WishType.Wisdom,       // Wisdom
            WishType.TreasureKey,  // Money (المفتاح)
            WishType.FlowerSpikes  // Flowers
        };

        // ربط الأزرار
        wishButtons[WishType.Agility] = athleticButton;
        wishButtons[WishType.Wife] = wifeButton;
        wishButtons[WishType.Wisdom] = wisdomButton;
        wishButtons[WishType.TreasureKey] = moneyButton;
        wishButtons[WishType.FlowerSpikes] = flowersButton;

        // ربط الصور
        wishImages[WishType.Agility] = athleticImage;
        wishImages[WishType.Wife] = wifeImage;
        wishImages[WishType.Wisdom] = wisdomImage;
        wishImages[WishType.TreasureKey] = moneyImage;
        wishImages[WishType.FlowerSpikes] = flowersImage;

        // Check if buttons are assigned
        int nullCount = 0;
        foreach (var kvp in wishButtons)
        {
            if (kvp.Value == null)
            {
                Debug.LogError($"❌ Button for {kvp.Key} is NULL!");
                nullCount++;
            }
            else
            {
                Debug.Log($"✓ Button for {kvp.Key}: {kvp.Value.name}");
            }
        }

        Debug.Log($"🧞 Buttons initialized: {wishButtons.Count - nullCount}/{wishButtons.Count} valid");
    }

    private void SetupButtons()
    {
        Debug.Log("🧞 GenieWishManager.SetupButtons called!");

        if (athleticButton != null)
        {
            athleticButton.onClick.RemoveAllListeners();
            athleticButton.onClick.AddListener(() => SelectWish(WishType.Agility));
            Debug.Log("✓ Athletic button listener added");
        }
        else
        {
            Debug.LogError("❌ athleticButton is NULL!");
        }

        if (wifeButton != null)
        {
            wifeButton.onClick.RemoveAllListeners();
            wifeButton.onClick.AddListener(() => SelectWish(WishType.Wife));
            Debug.Log("✓ Wife button listener added");
        }
        else
        {
            Debug.LogError("❌ wifeButton is NULL!");
        }

        if (wisdomButton != null)
        {
            wisdomButton.onClick.RemoveAllListeners();
            wisdomButton.onClick.AddListener(() => SelectWish(WishType.Wisdom));
            Debug.Log("✓ Wisdom button listener added");
        }
        else
        {
            Debug.LogError("❌ wisdomButton is NULL!");
        }

        if (moneyButton != null)
        {
            moneyButton.onClick.RemoveAllListeners();
            moneyButton.onClick.AddListener(() => SelectWish(WishType.TreasureKey));
            Debug.Log("✓ Money (TreasureKey) button listener added");
        }
        else
        {
            Debug.LogError("❌ moneyButton is NULL!");
        }

        if (flowersButton != null)
        {
            flowersButton.onClick.RemoveAllListeners();
            flowersButton.onClick.AddListener(() =>
            {
                Debug.Log("🌸🌸🌸 FLOWERS BUTTON CLICKED! 🌸🌸🌸");
                SelectWish(WishType.FlowerSpikes);
            });
            Debug.Log("✓ Flowers button listener added");
        }
        else
        {
            Debug.LogError("❌ flowersButton is NULL!");
        }

        Debug.Log("✅ All button listeners setup complete!");
    }

    public void ShowRandomWishes()
    {
        Debug.Log("🧞 ShowRandomWishes called!");
        Debug.Log($"🔍 isSelectingWish = {isSelectingWish}");

        // إعادة ضبط الحالة عند بداية عرض جديد
        isSelectingWish = false;

        // إعادة ربط الأزرار (للتأكد!)
        Debug.Log("🔧 Re-binding button listeners...");
        SetupButtons();

        // إخفاء كل الأزرار أولاً
        foreach (var kvp in wishButtons)
        {
            if (kvp.Value != null)
                kvp.Value.gameObject.SetActive(false);
        }

        // الأمنيات الغير مختارة
        List<WishType> unselectedWishes = availableWishes.Except(selectedWishes).ToList();

        Debug.Log($"📊 Total wishes: {availableWishes.Count}, Already selected: {selectedWishes.Count}, Remaining: {unselectedWishes.Count}");

        // اختر 3 أمنيات عشوائية (أو الباقي إذا أقل من 3)
        int wishesToShow = Mathf.Min(3, unselectedWishes.Count);

        if (wishesToShow == 0)
        {
            Debug.LogWarning("⚠️ No wishes remaining to show!");
            return;
        }

        // Shuffle بشكل أفضل
        System.Random rnd = new System.Random();
        List<WishType> randomWishes = unselectedWishes.OrderBy(x => rnd.Next()).Take(wishesToShow).ToList();

        Debug.Log($"🎲 Random wishes selected: {string.Join(", ", randomWishes)}");

        // أظهر الأمنيات المختارة
        int activatedCount = 0;
        foreach (WishType wish in randomWishes)
        {
            if (wishButtons.ContainsKey(wish) && wishButtons[wish] != null)
            {
                wishButtons[wish].gameObject.SetActive(true);
                activatedCount++;
                Debug.Log($"  ✓ Activated button for: {wish}");
            }
            else
            {
                Debug.LogWarning($"  ✗ Button not found for: {wish}");
            }
        }

        Debug.Log($"🧞 Showing {activatedCount} / {wishesToShow} wishes!");
    }

    private void SelectWish(WishType wish)
    {
        Debug.Log("═══════════════════════════════════════");
        Debug.Log($"🎯 SelectWish({wish}) called! Button was clicked!");
        Debug.Log("═══════════════════════════════════════");

        // منع إعادة الاختيار - حماية قوية!
        if (isSelectingWish)
        {
            Debug.LogWarning("⚠️ Already selecting a wish! Ignoring duplicate click.");
            return;
        }

        isSelectingWish = true;
        Debug.Log($"✅ Wish selected: {wish}");
        Debug.Log($"🔒 isSelectingWish set to TRUE - blocking further calls");

        // إضافة للمختارة
        if (!selectedWishes.Contains(wish))
        {
            selectedWishes.Add(wish);
        }

        // إخفاء Panel فوراً من هنا!
        GameObject geniePanel = gameObject;
        if (geniePanel != null)
        {
            geniePanel.SetActive(false);
            Debug.Log("🚫 GeniePanel hidden directly from SelectWish()!");
        }

        // تطبيق الأمنية
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            Debug.Log($"📞 Calling GameManager.OnWishSelected({wish})");
            gameManager.OnWishSelected(wish);
            Debug.Log("✅ GameManager.OnWishSelected() returned!");
        }
        else
        {
            Debug.LogError("❌ GameManager.Instance is NULL!");
        }
    }

    private void OnDisable()
    {
        // عند إخفاء Panel، أعد الحالة
        isSelectingWish = false;
    }

    // ═══════════════════════════════════════════════════════════════
    // Public Methods للربط في Inspector مباشرة!
    // ═══════════════════════════════════════════════════════════════

    public void OnAgilityButtonClick()
    {
        Debug.Log("🏃 Agility button clicked from Inspector!");
        SelectWish(WishType.Agility);
    }

    public void OnWifeButtonClick()
    {
        Debug.Log("👰 Wife button clicked from Inspector!");
        SelectWish(WishType.Wife);
    }

    public void OnWisdomButtonClick()
    {
        Debug.Log("🧠 Wisdom button clicked from Inspector!");
        SelectWish(WishType.Wisdom);
    }

    public void OnTreasureKeyButtonClick()
    {
        Debug.Log("🔑 TreasureKey button clicked from Inspector!");
        SelectWish(WishType.TreasureKey);
    }

    public void OnFlowersButtonClick()
    {
        Debug.Log("🌸 Flowers button clicked from Inspector!");
        SelectWish(WishType.FlowerSpikes);
    }

    // ═══════════════════════════════════════════════════════════════

    public void OnUndoTurn()
    {
        // إرجاع آخر أمنية مختارة
        if (selectedWishes.Count > 0)
        {
            WishType lastWish = selectedWishes[selectedWishes.Count - 1];
            selectedWishes.RemoveAt(selectedWishes.Count - 1);
            Debug.Log($"↩️ Undoing wish: {lastWish}");
        }
    }
}
