using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using GeniesGambit.Level;
using GeniesGambit.UI;

namespace GeniesGambit.Editor
{
    public class CaptchaSetupHelper : EditorWindow
    {
        [MenuItem("GeniesGambit/Setup Wisdom Wish Captcha")]
        static void ShowWindow()
        {
            var window = GetWindow<CaptchaSetupHelper>("Captcha Setup");
            window.minSize = new Vector2(400, 600);
            window.Show();
        }

        Vector2 scrollPosition;

        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("Wisdom Wish - Captcha Setup Helper", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            DrawInstructions();

            EditorGUILayout.Space(20);

            if (GUILayout.Button("1. Create CaptchaSpawner Prefab", GUILayout.Height(40)))
            {
                CreateCaptchaSpawnerPrefab();
            }

            EditorGUILayout.Space(10);

            if (GUILayout.Button("2. Create Sample Captcha Data", GUILayout.Height(40)))
            {
                CreateSampleCaptchaData();
            }

            EditorGUILayout.Space(10);

            if (GUILayout.Button("3. Open WD_Wisdom Asset", GUILayout.Height(40)))
            {
                OpenWisdomAsset();
            }

            EditorGUILayout.Space(20);
            DrawManualSteps();

            EditorGUILayout.EndScrollView();
        }

        void DrawInstructions()
        {
            EditorGUILayout.HelpBox(
                "This helper will create the CaptchaSpawner prefab and sample data.\n\n" +
                "Click the buttons below in order, then follow the manual steps.",
                MessageType.Info);
        }

        void DrawManualSteps()
        {
            EditorGUILayout.LabelField("Manual Steps Required:", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox(
                "After clicking button 3 (Open WD_Wisdom Asset):\n\n" +
                "1. Set 'Wish Type' to 'Wisdom' (currently shows FallingCoins)\n" +
                "2. Check 'Affects Monster' checkbox\n" +
                "3. Drag CaptchaSpawner prefab to 'Spawn Prefab' field\n" +
                "4. Save the asset (Ctrl+S)\n\n" +
                "Then test in Play Mode by selecting the Wisdom wish!",
                MessageType.Warning);
        }

        void CreateCaptchaSpawnerPrefab()
        {
            GameObject root = new GameObject("CaptchaSpawner");
            CaptchaManager manager = root.AddComponent<CaptchaManager>();

            GameObject canvasObj = new GameObject("Canvas");
            canvasObj.transform.SetParent(root.transform);
            
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            CanvasGroup canvasGroup = canvasObj.AddComponent<CanvasGroup>();
            CaptchaUI captchaUI = canvasObj.AddComponent<CaptchaUI>();

            GameObject background = new GameObject("Background");
            background.transform.SetParent(canvasObj.transform);
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.7f);
            RectTransform bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            GameObject promptObj = new GameObject("PromptText");
            promptObj.transform.SetParent(canvasObj.transform);
            TextMeshProUGUI promptText = promptObj.AddComponent<TextMeshProUGUI>();
            promptText.text = "Select all images with coins";
            promptText.fontSize = 36;
            promptText.alignment = TextAlignmentOptions.Center;
            promptText.color = Color.white;
            RectTransform promptRect = promptObj.GetComponent<RectTransform>();
            promptRect.anchorMin = new Vector2(0.5f, 1f);
            promptRect.anchorMax = new Vector2(0.5f, 1f);
            promptRect.pivot = new Vector2(0.5f, 1f);
            promptRect.anchoredPosition = new Vector2(0, -100);
            promptRect.sizeDelta = new Vector2(800, 100);

            GameObject timerObj = new GameObject("TimerText");
            timerObj.transform.SetParent(canvasObj.transform);
            TextMeshProUGUI timerText = timerObj.AddComponent<TextMeshProUGUI>();
            timerText.text = "Time: 20s";
            timerText.fontSize = 32;
            timerText.alignment = TextAlignmentOptions.Center;
            timerText.color = Color.white;
            RectTransform timerRect = timerObj.GetComponent<RectTransform>();
            timerRect.anchorMin = new Vector2(0.5f, 1f);
            timerRect.anchorMax = new Vector2(0.5f, 1f);
            timerRect.pivot = new Vector2(0.5f, 1f);
            timerRect.anchoredPosition = new Vector2(0, -180);
            timerRect.sizeDelta = new Vector2(300, 60);

            GameObject gridObj = new GameObject("GridContainer");
            gridObj.transform.SetParent(canvasObj.transform);
            RectTransform gridRect = gridObj.GetComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0.5f, 0.5f);
            gridRect.anchorMax = new Vector2(0.5f, 0.5f);
            gridRect.pivot = new Vector2(0.5f, 0.5f);
            gridRect.anchoredPosition = Vector2.zero;
            gridRect.sizeDelta = new Vector2(600, 600);
            GridLayoutGroup gridLayout = gridObj.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(180, 180);
            gridLayout.spacing = new Vector2(10, 10);
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.MiddleCenter;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 3;

            GameObject buttonObj = new GameObject("SubmitButton");
            buttonObj.transform.SetParent(canvasObj.transform);
            Button button = buttonObj.AddComponent<Button>();
            Image buttonImage = buttonObj.GetComponent<Image>();
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0f);
            buttonRect.anchorMax = new Vector2(0.5f, 0f);
            buttonRect.pivot = new Vector2(0.5f, 0f);
            buttonRect.anchoredPosition = new Vector2(0, 100);
            buttonRect.sizeDelta = new Vector2(300, 80);

            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform);
            TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "Submit";
            buttonText.fontSize = 32;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.black;
            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;

            GameObject slotPrefab = new GameObject("CaptchaImageSlot");
            Image slotImage = slotPrefab.AddComponent<Image>();
            slotImage.color = Color.white;
            RectTransform slotRect = slotPrefab.GetComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(180, 180);
            slotPrefab.AddComponent<Button>();

            string slotPrefabPath = "Assets/Prefabs/CaptchaImageSlot.prefab";
            PrefabUtility.SaveAsPrefabAsset(slotPrefab, slotPrefabPath);
            DestroyImmediate(slotPrefab);
            GameObject slotPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(slotPrefabPath);

            SerializedObject uiSO = new SerializedObject(captchaUI);
            uiSO.FindProperty("promptText").objectReferenceValue = promptText;
            uiSO.FindProperty("timerText").objectReferenceValue = timerText;
            uiSO.FindProperty("submitButton").objectReferenceValue = button;
            uiSO.FindProperty("gridContainer").objectReferenceValue = gridObj;
            uiSO.FindProperty("captchaImagePrefab").objectReferenceValue = slotPrefabAsset;
            uiSO.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
            uiSO.ApplyModifiedProperties();

            SerializedObject managerSO = new SerializedObject(manager);
            managerSO.FindProperty("minTriggerDelay").floatValue = 3f;
            managerSO.FindProperty("maxTriggerDelay").floatValue = 8f;
            managerSO.FindProperty("timeSlowFactor").floatValue = 0.3f;
            managerSO.FindProperty("captchaUI").objectReferenceValue = captchaUI;
            managerSO.ApplyModifiedProperties();

            canvasObj.SetActive(false);

            string prefabPath = "Assets/Prefabs/CaptchaSpawner.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            DestroyImmediate(root);

            EditorUtility.DisplayDialog("Success", 
                "CaptchaSpawner prefab created at:\n" + prefabPath + 
                "\n\nCaptchaImageSlot prefab created at:\n" + slotPrefabPath, 
                "OK");

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            EditorGUIUtility.PingObject(Selection.activeObject);
        }

        void CreateSampleCaptchaData()
        {
            string folderPath = "Assets/ScriptableObjects/Captchas";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Captchas");
            }

            CaptchaData captchaData = ScriptableObject.CreateInstance<CaptchaData>();
            captchaData.promptText = "Select all images with coins";
            captchaData.gridSize = 3;
            captchaData.timeLimit = 20f;
            captchaData.allImages = new Sprite[9];
            captchaData.correctIndices = new int[] { 0, 2, 5 };

            string assetPath = folderPath + "/CaptchaData_Sample.asset";
            AssetDatabase.CreateAsset(captchaData, assetPath);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("Success", 
                "Sample Captcha Data created at:\n" + assetPath + 
                "\n\nNOTE: You need to add sprites to the 'All Images' array!\n" +
                "Add 9 sprites (mix of coins and other objects).\n" +
                "Correct indices are set to 0, 2, 5.", 
                "OK");

            Selection.activeObject = captchaData;
            EditorGUIUtility.PingObject(captchaData);
        }

        void OpenWisdomAsset()
        {
            string assetPath = "Assets/ScriptableObjects/Wishes/WD_Wisdom.asset";
            Object wisdomAsset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            
            if (wisdomAsset != null)
            {
                Selection.activeObject = wisdomAsset;
                EditorGUIUtility.PingObject(wisdomAsset);
                
                EditorUtility.DisplayDialog("Manual Steps Required", 
                    "WD_Wisdom asset is now selected in Inspector.\n\n" +
                    "Please complete these steps:\n\n" +
                    "1. Change 'Wish Type' from 'FallingCoins' to 'Wisdom'\n" +
                    "2. Check the 'Affects Monster' checkbox\n" +
                    "3. Drag the CaptchaSpawner prefab to 'Spawn Prefab'\n" +
                    "4. Save (Ctrl+S)\n\n" +
                    "Then test in Play Mode!", 
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Could not find WD_Wisdom asset at:\n" + assetPath, "OK");
            }
        }
    }
}
