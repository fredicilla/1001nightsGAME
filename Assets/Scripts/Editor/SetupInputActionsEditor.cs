using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;

public class SetupInputActionsEditor : EditorWindow
{
    [MenuItem("Tools/Setup Input Actions (One Click)")]
    public static void ShowWindow()
    {
        SetupInputActions();
    }
    
    private static void SetupInputActions()
    {
        Debug.Log("=== بدء إعداد Input Actions ===");
        
        // Load Input Actions asset
        string assetPath = "Assets/InputSystem_Actions.inputactions";
        var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(assetPath);
        
        if (inputActions == null)
        {
            Debug.LogError("لم يتم العثور على InputSystem_Actions.inputactions في Assets/");
            EditorUtility.DisplayDialog("خطأ", "لم يتم العثور على InputSystem_Actions.inputactions", "حسناً");
            return;
        }
        
        Debug.Log("✅ تم العثور على Input Actions Asset");
        
        // Find or create "Player" action map
        var playerMap = inputActions.FindActionMap("Player");
        if (playerMap == null)
        {
            playerMap = inputActions.AddActionMap("Player");
            Debug.Log("✅ تم إنشاء Player Action Map");
        }
        else
        {
            Debug.Log("✅ Player Action Map موجود مسبقاً");
        }
        
        // Setup Move action
        SetupMoveAction(playerMap);
        
        // Setup Jump action
        SetupJumpAction(playerMap);
        
        // Setup Shoot action
        SetupShootAction(playerMap);
        
        // Save changes
        EditorUtility.SetDirty(inputActions);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("=== ✅ اكتمل إعداد Input Actions بنجاح! ===");
        EditorUtility.DisplayDialog(
            "نجح!", 
            "تم إعداد Input Actions بنجاح!\n\n" +
            "الآن فقط:\n" +
            "1. افتح PlayerComplete prefab\n" +
            "2. في Player Input component → Events\n" +
            "3. اربط:\n" +
            "   - Move → PlayerController.OnMove\n" +
            "   - Jump → PlayerController.OnJump\n" +
            "   - Shoot → PlayerController.OnShoot\n\n" +
            "ثم اضغط Play!", 
            "رائع!"
        );
    }
    
    private static void SetupMoveAction(InputActionMap playerMap)
    {
        var moveAction = playerMap.FindAction("Move");
        if (moveAction == null)
        {
            moveAction = playerMap.AddAction("Move", InputActionType.Value, binding: null, interactions: null, processors: null, groups: null, expectedControlLayout: "Vector2");
        }
        
        // Remove existing bindings
        for (int i = moveAction.bindings.Count - 1; i >= 0; i--)
        {
            moveAction.ChangeBinding(i).Erase();
        }
        
        // Add WASD composite
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        
        // Add Arrow Keys composite
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");
        
        Debug.Log("✅ تم إعداد Move Action (WASD + Arrows)");
    }
    
    private static void SetupJumpAction(InputActionMap playerMap)
    {
        var jumpAction = playerMap.FindAction("Jump");
        if (jumpAction == null)
        {
            jumpAction = playerMap.AddAction("Jump", InputActionType.Button);
        }
        
        // Remove existing bindings
        for (int i = jumpAction.bindings.Count - 1; i >= 0; i--)
        {
            jumpAction.ChangeBinding(i).Erase();
        }
        
        // Add Space binding
        jumpAction.AddBinding("<Keyboard>/space");
        
        Debug.Log("✅ تم إعداد Jump Action (Space)");
    }
    
    private static void SetupShootAction(InputActionMap playerMap)
    {
        var shootAction = playerMap.FindAction("Shoot");
        if (shootAction == null)
        {
            shootAction = playerMap.AddAction("Shoot", InputActionType.Button);
        }
        
        // Remove existing bindings
        for (int i = shootAction.bindings.Count - 1; i >= 0; i--)
        {
            shootAction.ChangeBinding(i).Erase();
        }
        
        // Add Mouse Left binding
        shootAction.AddBinding("<Mouse>/leftButton");
        
        // Add E key as alternative
        shootAction.AddBinding("<Keyboard>/e");
        
        Debug.Log("✅ تم إعداد Shoot Action (Mouse Left + E)");
    }
}
