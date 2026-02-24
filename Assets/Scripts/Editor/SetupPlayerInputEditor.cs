using BossFight;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEditor.Events;

public class SetupPlayerInputEditor : EditorWindow
{
    [MenuItem("Tools/Setup Player Input Events (One Click)")]
    public static void SetupPlayerInputEvents()
    {
        Debug.Log("=== بدء إعداد Player Input Events ===");

        // Load PlayerComplete prefab
        string prefabPath = "Assets/Prefabs/PlayerComplete.prefab";
        GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (prefabAsset == null)
        {
            Debug.LogError("لم يتم العثور على PlayerComplete.prefab");
            EditorUtility.DisplayDialog("خطأ", "لم يتم العثور على PlayerComplete.prefab في Assets/Prefabs/", "حسناً");
            return;
        }

        Debug.Log("✅ تم العثور على PlayerComplete prefab");

        // Get PlayerInput component
        PlayerInput playerInput = prefabAsset.GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("PlayerInput component غير موجود على PlayerComplete!");
            EditorUtility.DisplayDialog("خطأ", "PlayerInput component غير موجود!", "حسناً");
            return;
        }

        Debug.Log("✅ تم العثور على PlayerInput component");

        // Get PlayerController component
        PlayerController playerController = prefabAsset.GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerController component غير موجود على PlayerComplete!");
            EditorUtility.DisplayDialog("خطأ", "PlayerController component غير موجود!", "حسناً");
            return;
        }

        Debug.Log("✅ تم العثور على PlayerController component");

        // Set behavior to Invoke Unity Events
        SerializedObject serializedObject = new SerializedObject(playerInput);
        SerializedProperty notificationBehavior = serializedObject.FindProperty("m_NotificationBehavior");
        notificationBehavior.intValue = 2; // InvokeUnityEvents
        serializedObject.ApplyModifiedProperties();

        Debug.Log("✅ تم ضبط Behavior على Invoke Unity Events");

        // Try to set default action map
        SerializedProperty defaultActionMap = serializedObject.FindProperty("m_DefaultActionMap");
        defaultActionMap.stringValue = "Player";
        serializedObject.ApplyModifiedProperties();

        Debug.Log("✅ تم ضبط Default Action Map على Player");

        // Mark prefab as dirty to save changes
        EditorUtility.SetDirty(prefabAsset);
        PrefabUtility.SavePrefabAsset(prefabAsset);

        Debug.Log("=== ✅ اكتمل إعداد Player Input! ===");

        EditorUtility.DisplayDialog(
            "نجح!",
            "تم إعداد Player Input بنجاح!\n\n" +
            "⚠️ ملاحظة مهمة:\n" +
            "للأسف، Unity لا يسمح بربط Events تلقائياً.\n\n" +
            "الخطوة الأخيرة (يدوياً):\n" +
            "1. افتح PlayerComplete prefab\n" +
            "2. في Player Input → Events → Player\n" +
            "3. اربط:\n" +
            "   Move → اسحب PlayerComplete → PlayerController.OnMove\n" +
            "   Jump → اسحب PlayerComplete → PlayerController.OnJump\n" +
            "   Shoot → اسحب PlayerComplete → PlayerController.OnShoot\n\n" +
            "هذا سريع جداً! (دقيقة واحدة)",
            "فهمت"
        );
    }
}
