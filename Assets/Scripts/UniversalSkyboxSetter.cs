using UnityEngine;
using UnityEngine.Rendering;

public class UniversalSkyboxSetter : MonoBehaviour
{
    [Header("Skybox Material")]
    [SerializeField] Material skyboxMaterial;

    [Header("Skybox Settings")]
    [SerializeField] [Range(0f, 8f)] float exposure = 2.0f;
    [SerializeField] Color tintColor = Color.white;

    [Header("Advanced Options")]
    [SerializeField] bool disableFog = true;
    [SerializeField] bool removeCameraSkyboxComponent = true;
    [SerializeField] bool forceAmbientModeToSkybox = true;
    [SerializeField] bool debugLogging = true;

    void Awake()
    {
        ApplySkybox("Awake");
    }

    void Start()
    {
        ApplySkybox("Start");
    }

    void ApplySkybox(string caller)
    {
        if (debugLogging)
            Debug.Log($"[UniversalSkyboxSetter] ApplySkybox called from: {caller}");

        if (skyboxMaterial == null)
        {
            Debug.LogError("[UniversalSkyboxSetter] Skybox material is NULL! Cannot apply skybox.");
            return;
        }

        ConfigureMaterialProperties();
        
        RemoveCameraSkyboxComponentIfNeeded();
        
        ApplySkyboxToRenderSettings();
        
        DisableFogIfNeeded();
        
        UpdateLightingEnvironment();

        if (debugLogging)
            Debug.Log($"[UniversalSkyboxSetter] Skybox applied successfully: {skyboxMaterial.name}");
    }

    void ConfigureMaterialProperties()
    {
        if (skyboxMaterial.HasProperty("_Exposure"))
        {
            skyboxMaterial.SetFloat("_Exposure", exposure);
            if (debugLogging)
                Debug.Log($"  Set _Exposure = {exposure}");
        }

        if (skyboxMaterial.HasProperty("_Tint"))
        {
            skyboxMaterial.SetColor("_Tint", tintColor);
            if (debugLogging)
                Debug.Log($"  Set _Tint = {tintColor}");
        }

        if (skyboxMaterial.HasProperty("_SkyTint"))
        {
            skyboxMaterial.SetColor("_SkyTint", tintColor);
            if (debugLogging)
                Debug.Log($"  Set _SkyTint = {tintColor}");
        }
    }

    void RemoveCameraSkyboxComponentIfNeeded()
    {
        if (!removeCameraSkyboxComponent)
            return;

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Skybox camSkybox = mainCam.GetComponent<Skybox>();
            if (camSkybox != null)
            {
                if (debugLogging)
                    Debug.LogWarning($"  Removing Skybox component from camera '{mainCam.name}'");
                
                Destroy(camSkybox);
            }
        }
    }

    void ApplySkyboxToRenderSettings()
    {
        RenderSettings.skybox = skyboxMaterial;
        
        if (forceAmbientModeToSkybox)
        {
            RenderSettings.ambientMode = AmbientMode.Skybox;
            if (debugLogging)
                Debug.Log($"  Set ambient mode to Skybox");
        }

        Camera mainCam = Camera.main;
        if (mainCam != null && mainCam.clearFlags != CameraClearFlags.Skybox)
        {
            if (debugLogging)
                Debug.LogWarning($"  Camera clear flags is {mainCam.clearFlags}, consider setting to Skybox");
        }
    }

    void DisableFogIfNeeded()
    {
        if (disableFog && RenderSettings.fog)
        {
            if (debugLogging)
                Debug.LogWarning($"  Disabling fog (was enabled with color {RenderSettings.fogColor})");
            
            RenderSettings.fog = false;
        }
    }

    void UpdateLightingEnvironment()
    {
        DynamicGI.UpdateEnvironment();
        
        if (debugLogging)
            Debug.Log("  Updated lighting environment");
    }

    [ContextMenu("Force Apply Skybox")]
    public void ForceApplySkybox()
    {
        Debug.Log("[UniversalSkyboxSetter] Force applying skybox...");
        ApplySkybox("Force Apply");
    }

    [ContextMenu("Validate Camera Settings")]
    void ValidateCameraSettings()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("No main camera found!");
            return;
        }

        Debug.Log("=== Camera Validation ===");
        Debug.Log($"Camera: {mainCam.name}");
        Debug.Log($"Clear Flags: {mainCam.clearFlags}");
        Debug.Log($"Background Color: {mainCam.backgroundColor}");
        
        Skybox camSkybox = mainCam.GetComponent<Skybox>();
        if (camSkybox != null)
        {
            Debug.LogWarning($"WARNING: Camera has Skybox component (material: {(camSkybox.material != null ? camSkybox.material.name : "NULL")})");
        }
        else
        {
            Debug.Log("OK: No Skybox component on camera");
        }
    }

    [ContextMenu("Validate Current Skybox")]
    void ValidateCurrentSkybox()
    {
        Debug.Log("=== Skybox Validation ===");
        Debug.Log($"RenderSettings.skybox: {(RenderSettings.skybox != null ? RenderSettings.skybox.name : "NULL")}");
        
        if (RenderSettings.skybox != null)
        {
            Debug.Log($"  Shader: {RenderSettings.skybox.shader.name}");
            
            if (RenderSettings.skybox.HasProperty("_Exposure"))
                Debug.Log($"  Exposure: {RenderSettings.skybox.GetFloat("_Exposure")}");
            
            if (RenderSettings.skybox.HasProperty("_Tint"))
                Debug.Log($"  Tint: {RenderSettings.skybox.GetColor("_Tint")}");
        }
        
        Debug.Log($"Ambient Mode: {RenderSettings.ambientMode}");
        Debug.Log($"Fog Enabled: {RenderSettings.fog}");
        
        if (RenderSettings.fog)
            Debug.Log($"  Fog Color: {RenderSettings.fogColor}");
    }
}
