using UnityEngine;

public class BossFightSkyboxSetter : MonoBehaviour
{
    [Header("Skybox Material")]
    [SerializeField] Material skyboxMaterial;

    [Header("Skybox Settings")]
    [SerializeField] [Range(0f, 8f)] float exposure = 0.8f;
    [SerializeField] Color tintColor = new Color(0.5f, 0.5f, 0.7f, 1f);

    void Awake()
    {
        // Disabled - causes overexposure issues with 3D renderer
        // CheckCurrentSkybox("BEFORE Awake");
        // ApplySkybox("Awake");
        // CheckCurrentSkybox("AFTER Awake");
    }

    void Start()
    {
        // Disabled - causes overexposure issues with 3D renderer
        // CheckCurrentSkybox("BEFORE Start");
        // ApplySkybox("Start");
        // CheckCurrentSkybox("AFTER Start");
    }

    void Update()
    {
        // Disabled - no longer needed
        /*
        if (Time.frameCount % 60 == 0)
        {
            CheckCurrentSkybox("Update Frame " + Time.frameCount);
        }
        */
    }

    void CheckCurrentSkybox(string phase)
    {
        Debug.Log($"📊 ══════ {phase} ══════");
        
        Debug.Log($"🎯 RenderSettings.skybox = {(RenderSettings.skybox != null ? RenderSettings.skybox.name : "NULL")}");
        if (RenderSettings.skybox != null)
        {
            Debug.Log($"   └─ Shader: {RenderSettings.skybox.shader.name}");
            if (RenderSettings.skybox.HasProperty("_Exposure"))
                Debug.Log($"   └─ Exposure: {RenderSettings.skybox.GetFloat("_Exposure")}");
            if (RenderSettings.skybox.HasProperty("_Tint"))
                Debug.Log($"   └─ Tint: {RenderSettings.skybox.GetColor("_Tint")}");
        }
        
        Debug.Log($"🌍 RenderSettings.ambientMode = {RenderSettings.ambientMode}");
        
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Debug.Log($"📷 Main Camera found: {mainCam.name}");
            Debug.Log($"   └─ ClearFlags: {mainCam.clearFlags}");
            
            Skybox camSkybox = mainCam.GetComponent<Skybox>();
            if (camSkybox != null)
            {
                Debug.LogWarning($"⚠️ Camera has Skybox component!");
                Debug.LogWarning($"   └─ Material: {(camSkybox.material != null ? camSkybox.material.name : "NULL")}");
                Debug.LogWarning($"   └─ Enabled: {camSkybox.enabled}");
                Debug.LogWarning($"   └─ ⚠️ هذا يتجاوز RenderSettings!");
            }
            else
            {
                Debug.Log($"   ✅ No Skybox component on camera (good!)");
            }
        }
        else
        {
            Debug.LogError("❌ Camera.main is NULL!");
        }
        
        Debug.Log($"🎨 skyboxMaterial field = {(skyboxMaterial != null ? skyboxMaterial.name : "NULL")}");
        if (skyboxMaterial != null)
        {
            Debug.Log($"   └─ Shader: {skyboxMaterial.shader.name}");
        }
        
        Debug.Log("════════════════════════════════════════");
    }

    void ApplySkybox(string caller)
    {
        Debug.Log($"🔧 [BossFightSkybox] ApplySkybox called from: {caller}");
        
        if (skyboxMaterial == null)
        {
            Debug.LogError("❌ skyboxMaterial is NULL! لا يمكن تطبيق السماء!");
            Debug.LogError("   السبب: لم يتم تعيين Material في Inspector");
            return;
        }

        Debug.Log($"✅ skyboxMaterial assigned: {skyboxMaterial.name}");
        
        try
        {
            if (skyboxMaterial.HasProperty("_Exposure"))
            {
                skyboxMaterial.SetFloat("_Exposure", exposure);
                Debug.Log($"   ✓ Set _Exposure = {exposure}");
            }
            else
            {
                Debug.LogWarning($"   ⚠️ Material doesn't have _Exposure property");
            }
            
            if (skyboxMaterial.HasProperty("_Tint"))
            {
                skyboxMaterial.SetColor("_Tint", tintColor);
                Debug.Log($"   ✓ Set _Tint = {tintColor}");
            }
            else
            {
                Debug.LogWarning($"   ⚠️ Material doesn't have _Tint property");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error setting material properties: {e.Message}");
        }
        
        Material oldSkybox = RenderSettings.skybox;
        Debug.Log($"🔄 Changing RenderSettings.skybox from '{(oldSkybox != null ? oldSkybox.name : "NULL")}' to '{skyboxMaterial.name}'");
        
        RenderSettings.skybox = skyboxMaterial;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        
        Debug.Log($"   ✓ RenderSettings.skybox = {RenderSettings.skybox.name}");
        Debug.Log($"   ✓ RenderSettings.ambientMode = {RenderSettings.ambientMode}");
        
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Skybox camSkybox = mainCam.GetComponent<Skybox>();
            if (camSkybox != null)
            {
                Debug.LogWarning("⚠️ Found Skybox component on camera - REMOVING IT!");
                Debug.LogWarning($"   Material was: {(camSkybox.material != null ? camSkybox.material.name : "NULL")}");
                Destroy(camSkybox);
                Debug.Log("   ✓ Skybox component destroyed!");
            }
            else
            {
                Debug.Log("   ✓ No Skybox component on camera");
            }
        }
        
        DynamicGI.UpdateEnvironment();
        Debug.Log("   ✓ DynamicGI.UpdateEnvironment() called");
        
        Debug.Log($"✅✅✅ Skybox تم تطبيقه بنجاح! ✅✅✅");
    }

    [ContextMenu("Force Apply Skybox")]
    void ForceApplySkybox()
    {
        Debug.Log("🔨 Force Apply Skybox (من Context Menu)");
        CheckCurrentSkybox("BEFORE Force Apply");
        ApplySkybox("Force Apply");
        CheckCurrentSkybox("AFTER Force Apply");
    }

    [ContextMenu("Check Current State")]
    void CheckState()
    {
        CheckCurrentSkybox("Manual Check");
    }
}
