using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using GeniesGambit.UI;

public static class ProgressionBarSetup
{
    [MenuItem("Tools/Setup Progression Bar in Scene")]
    public static void SetupProgressionBar()
    {
        var iterationUI = Object.FindFirstObjectByType<IterationUI>(FindObjectsInactive.Include);
        if (iterationUI == null)
        {
            Debug.LogError("[ProgressionBarSetup] No IterationUI found in scene. Make sure SampleScene is open.");
            return;
        }

        Sprite lockedSprite    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Progress bar (locked).png");
        Sprite wishSprite      = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Progress bar (wish).png");
        Sprite unlockedSprite  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Progress bar (unlocked).png");
        Sprite completedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Progress bar (completed).png");
        Sprite bossLockedSprite= AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/lockedGenie.png");
        Sprite bossReadySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/genieBar.png");

        // --- Create or reuse ProgressionBar container under IterationUI ---
        Transform existingBar = iterationUI.transform.Find("ProgressionBar");
        if (existingBar != null)
        {
            Undo.DestroyObjectImmediate(existingBar.gameObject);
            Debug.Log("[ProgressionBarSetup] Removed existing ProgressionBar, rebuilding.");
        }

        GameObject barRoot = new GameObject("ProgressionBar");
        Undo.RegisterCreatedObjectUndo(barRoot, "Create ProgressionBar");
        barRoot.transform.SetParent(iterationUI.transform, false);

        var barRect = barRoot.AddComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0f, 0f);
        barRect.anchorMax = new Vector2(1f, 0f);
        barRect.pivot = new Vector2(0f, 0f);
        barRect.anchoredPosition = new Vector2(0f, -90f);
        barRect.sizeDelta = new Vector2(0f, 60f);

        var hlg = barRoot.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.spacing = 6f;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;

        // 7 iteration slots + 1 boss slot = 8 total
        var slots = new System.Collections.Generic.List<Image>();
        for (int i = 0; i < 8; i++)
        {
            bool isBoss = (i == 7);
            string slotName = isBoss ? "Slot_Boss" : $"Slot_{i + 1}";

            GameObject slotGO = new GameObject(slotName);
            Undo.RegisterCreatedObjectUndo(slotGO, $"Create {slotName}");
            slotGO.transform.SetParent(barRoot.transform, false);

            var slotRect = slotGO.AddComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(56f, 56f);

            var img = slotGO.AddComponent<Image>();
            img.sprite = isBoss ? bossLockedSprite : lockedSprite;
            img.preserveAspect = true;
            slots.Add(img);
        }

        // --- Wire up IterationUI serialized fields via SerializedObject ---
        var so = new SerializedObject(iterationUI);

        SerializedProperty slotsProp = so.FindProperty("progressionSlotImages");
        slotsProp.ClearArray();
        for (int i = 0; i < slots.Count; i++)
        {
            slotsProp.InsertArrayElementAtIndex(i);
            slotsProp.GetArrayElementAtIndex(i).objectReferenceValue = slots[i];
        }

        so.FindProperty("progressLockedSprite").objectReferenceValue   = lockedSprite;
        so.FindProperty("progressWishSprite").objectReferenceValue     = wishSprite;
        so.FindProperty("progressUnlockedSprite").objectReferenceValue = unlockedSprite;
        so.FindProperty("progressCompletedSprite").objectReferenceValue= completedSprite;
        so.FindProperty("bossLockedSprite").objectReferenceValue       = bossLockedSprite;
        so.FindProperty("bossReadySprite").objectReferenceValue        = bossReadySprite;

        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(iterationUI);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[ProgressionBarSetup] Done! 8 progression slots created and all sprites wired.");
    }
}
