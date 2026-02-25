using UnityEngine;

public class SkyboxTester : MonoBehaviour
{
    [Header("Test Materials")]
    [SerializeField] Material testMaterial;

    [Header("Runtime Testing")]
    [SerializeField] bool applyOnStart = true;

    void Start()
    {
        if (applyOnStart && testMaterial != null)
        {
            Debug.Log($"[SkyboxTester] Applying test material: {testMaterial.name}");
            RenderSettings.skybox = testMaterial;
            DynamicGI.UpdateEnvironment();
        }
    }

    [ContextMenu("Apply Test Material")]
    public void ApplyTestMaterial()
    {
        if (testMaterial == null)
        {
            Debug.LogError("[SkyboxTester] No test material assigned!");
            return;
        }

        Debug.Log($"[SkyboxTester] Manually applying: {testMaterial.name}");
        RenderSettings.skybox = testMaterial;
        DynamicGI.UpdateEnvironment();
        
        Debug.Log($"Current skybox: {RenderSettings.skybox.name}");
        Debug.Log($"Shader: {RenderSettings.skybox.shader.name}");
        
        if (RenderSettings.skybox.HasProperty("_TopColor"))
            Debug.Log($"Top Color: {RenderSettings.skybox.GetColor("_TopColor")}");
        if (RenderSettings.skybox.HasProperty("_HorizonColor"))
            Debug.Log($"Horizon Color: {RenderSettings.skybox.GetColor("_HorizonColor")}");
        if (RenderSettings.skybox.HasProperty("_BottomColor"))
            Debug.Log($"Bottom Color: {RenderSettings.skybox.GetColor("_BottomColor")}");
    }

    [ContextMenu("Print Current Skybox")]
    public void PrintCurrentSkybox()
    {
        Debug.Log("=== Current Skybox State ===");
        Debug.Log($"RenderSettings.skybox: {(RenderSettings.skybox != null ? RenderSettings.skybox.name : "NULL")}");
        
        if (RenderSettings.skybox != null)
        {
            Debug.Log($"Shader: {RenderSettings.skybox.shader.name}");
            
            int propCount = RenderSettings.skybox.shader.GetPropertyCount();
            Debug.Log($"Property Count: {propCount}");
            
            for (int i = 0; i < propCount; i++)
            {
                string propName = RenderSettings.skybox.shader.GetPropertyName(i);
                var propType = RenderSettings.skybox.shader.GetPropertyType(i);
                
                Debug.Log($"  [{i}] {propName} ({propType})");
                
                try
                {
                    if (propType == UnityEngine.Rendering.ShaderPropertyType.Color)
                    {
                        var colorValue = RenderSettings.skybox.GetColor(propName);
                        Debug.Log($"      = {colorValue}");
                    }
                    else if (propType == UnityEngine.Rendering.ShaderPropertyType.Float || 
                             propType == UnityEngine.Rendering.ShaderPropertyType.Range)
                    {
                        var floatValue = RenderSettings.skybox.GetFloat(propName);
                        Debug.Log($"      = {floatValue}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"      Error: {e.Message}");
                }
            }
        }
        
        Debug.Log($"Ambient Mode: {RenderSettings.ambientMode}");
        Debug.Log($"Fog: {RenderSettings.fog}");
        
        if (Camera.main != null)
        {
            Debug.Log($"Camera Clear Flags: {Camera.main.clearFlags}");
        }
    }
}
