using UnityEngine;
using UnityEngine.Rendering;

public class SkyboxDiagnostics : MonoBehaviour
{
    [ContextMenu("Run Full Diagnostics")]
    public void RunFullDiagnostics()
    {
        Debug.Log("╔════════════════════════════════════════════════════════════╗");
        Debug.Log("║          SKYBOX DIAGNOSTIC REPORT - FULL SCAN              ║");
        Debug.Log("╚════════════════════════════════════════════════════════════╝");
        Debug.Log("");

        CheckRenderSettings();
        CheckCameraSettings();
        CheckPostProcessing();
        CheckFogSettings();
        CheckLightingSettings();
        CheckColorSpace();
        CheckURPSettings();
        CheckMaterialProperties();
        
        Debug.Log("");
        Debug.Log("╔════════════════════════════════════════════════════════════╗");
        Debug.Log("║                DIAGNOSTIC REPORT COMPLETE                  ║");
        Debug.Log("╚════════════════════════════════════════════════════════════╝");
    }

    void CheckRenderSettings()
    {
        Debug.Log("┌─────────────────────────────────────────────────────────┐");
        Debug.Log("│ 1. RENDER SETTINGS                                      │");
        Debug.Log("└─────────────────────────────────────────────────────────┘");
        
        Debug.Log($"  Skybox Material: {(RenderSettings.skybox != null ? RenderSettings.skybox.name : "NULL")}");
        
        if (RenderSettings.skybox != null)
        {
            Debug.Log($"    └─ Shader: {RenderSettings.skybox.shader.name}");
            
            string[] commonProps = { "_Tint", "_Exposure", "_SkyTint", "_AtmosphereThickness", "_SunSize" };
            foreach (string prop in commonProps)
            {
                if (RenderSettings.skybox.HasProperty(prop))
                {
                    if (prop.Contains("Color") || prop.Contains("Tint"))
                        Debug.Log($"    └─ {prop}: {RenderSettings.skybox.GetColor(prop)}");
                    else if (RenderSettings.skybox.shader.FindPropertyIndex(prop) >= 0)
                    {
                        var propType = RenderSettings.skybox.shader.GetPropertyType(RenderSettings.skybox.shader.FindPropertyIndex(prop));
                        if (propType == UnityEngine.Rendering.ShaderPropertyType.Float || propType == UnityEngine.Rendering.ShaderPropertyType.Range)
                            Debug.Log($"    └─ {prop}: {RenderSettings.skybox.GetFloat(prop)}");
                    }
                }
            }
        }
        
        Debug.Log($"  Ambient Mode: {RenderSettings.ambientMode}");
        Debug.Log($"  Ambient Light: {RenderSettings.ambientLight}");
        Debug.Log($"  Ambient Sky Color: {RenderSettings.ambientSkyColor}");
        Debug.Log($"  Ambient Equator Color: {RenderSettings.ambientEquatorColor}");
        Debug.Log($"  Ambient Ground Color: {RenderSettings.ambientGroundColor}");
        Debug.Log("");
    }

    void CheckCameraSettings()
    {
        Debug.Log("┌─────────────────────────────────────────────────────────┐");
        Debug.Log("│ 2. CAMERA SETTINGS                                      │");
        Debug.Log("└─────────────────────────────────────────────────────────┘");
        
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Debug.Log($"  Main Camera: {mainCam.name}");
            Debug.Log($"    └─ Clear Flags: {mainCam.clearFlags}");
            Debug.Log($"    └─ Background Color: {mainCam.backgroundColor}");
            Debug.Log($"    └─ Allow HDR: {mainCam.allowHDR}");
            Debug.Log($"    └─ Allow MSAA: {mainCam.allowMSAA}");
            
            Skybox camSkybox = mainCam.GetComponent<Skybox>();
            if (camSkybox != null)
            {
                Debug.LogWarning("  ⚠️ WARNING: Camera has Skybox component!");
                Debug.LogWarning($"    └─ Material: {(camSkybox.material != null ? camSkybox.material.name : "NULL")}");
                Debug.LogWarning($"    └─ Enabled: {camSkybox.enabled}");
                Debug.LogWarning("    └─ This will override RenderSettings.skybox!");
            }
            else
            {
                Debug.Log("  ✓ No Skybox component on camera (correct)");
            }
        }
        else
        {
            Debug.LogError("  ❌ ERROR: Camera.main is NULL!");
        }
        Debug.Log("");
    }

    void CheckPostProcessing()
    {
        Debug.Log("┌─────────────────────────────────────────────────────────┐");
        Debug.Log("│ 3. POST-PROCESSING VOLUMES                              │");
        Debug.Log("└─────────────────────────────────────────────────────────┘");
        
        Volume[] volumes = FindObjectsByType<Volume>(FindObjectsSortMode.None);
        
        if (volumes.Length == 0)
        {
            Debug.Log("  ✓ No Post-Processing Volumes found");
        }
        else
        {
            foreach (Volume volume in volumes)
            {
                Debug.Log($"  Volume: {volume.gameObject.name}");
                Debug.Log($"    └─ Enabled: {volume.enabled}");
                Debug.Log($"    └─ Is Global: {volume.isGlobal}");
                Debug.Log($"    └─ Weight: {volume.weight}");
                Debug.Log($"    └─ Priority: {volume.priority}");
                Debug.Log($"    └─ Profile: {(volume.profile != null ? volume.profile.name : "NULL")}");
                
                if (volume.profile != null && volume.profile.components.Count > 0)
                {
                    Debug.LogWarning($"    ⚠️ WARNING: Volume has {volume.profile.components.Count} component(s)");
                    foreach (var component in volume.profile.components)
                    {
                        Debug.LogWarning($"      └─ {component.GetType().Name} (active: {component.active})");
                    }
                }
            }
        }
        Debug.Log("");
    }

    void CheckFogSettings()
    {
        Debug.Log("┌─────────────────────────────────────────────────────────┐");
        Debug.Log("│ 4. FOG SETTINGS                                         │");
        Debug.Log("└─────────────────────────────────────────────────────────┘");
        
        Debug.Log($"  Fog Enabled: {RenderSettings.fog}");
        
        if (RenderSettings.fog)
        {
            Debug.LogWarning("  ⚠️ WARNING: Fog is enabled!");
            Debug.LogWarning($"    └─ Fog Color: {RenderSettings.fogColor}");
            Debug.LogWarning($"    └─ Fog Mode: {RenderSettings.fogMode}");
            Debug.LogWarning($"    └─ Fog Density: {RenderSettings.fogDensity}");
            Debug.LogWarning($"    └─ Fog Start Distance: {RenderSettings.fogStartDistance}");
            Debug.LogWarning($"    └─ Fog End Distance: {RenderSettings.fogEndDistance}");
            Debug.LogWarning("    └─ Fog can override skybox color!");
        }
        else
        {
            Debug.Log("  ✓ Fog is disabled (correct)");
        }
        Debug.Log("");
    }

    void CheckLightingSettings()
    {
        Debug.Log("┌─────────────────────────────────────────────────────────┐");
        Debug.Log("│ 5. LIGHTING SETTINGS                                    │");
        Debug.Log("└─────────────────────────────────────────────────────────┘");
        
        Debug.Log($"  Sun Source: {(RenderSettings.sun != null ? RenderSettings.sun.name : "NULL")}");
        Debug.Log($"  Ambient Intensity: {RenderSettings.ambientIntensity}");
        Debug.Log($"  Reflection Intensity: {RenderSettings.reflectionIntensity}");
        Debug.Log($"  Default Reflection Mode: {RenderSettings.defaultReflectionMode}");
        Debug.Log("");
    }

    void CheckColorSpace()
    {
        Debug.Log("┌─────────────────────────────────────────────────────────┐");
        Debug.Log("│ 6. COLOR SPACE                                          │");
        Debug.Log("└─────────────────────────────────────────────────────────┘");
        
        Debug.Log($"  Active Color Space: {QualitySettings.activeColorSpace}");
        
        if (QualitySettings.activeColorSpace != ColorSpace.Linear)
        {
            Debug.LogWarning("  ⚠️ WARNING: Not using Linear color space");
            Debug.LogWarning("    └─ This may affect color appearance");
        }
        else
        {
            Debug.Log("  ✓ Using Linear color space (recommended for URP)");
        }
        Debug.Log("");
    }

    void CheckURPSettings()
    {
        Debug.Log("┌─────────────────────────────────────────────────────────┐");
        Debug.Log("│ 7. URP PIPELINE SETTINGS                                │");
        Debug.Log("└─────────────────────────────────────────────────────────┘");
        
        var pipeline = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
        
        if (pipeline != null)
        {
            Debug.Log($"  Render Pipeline: {pipeline.name}");
            Debug.Log($"  Pipeline Type: {pipeline.GetType().Name}");
        }
        else
        {
            Debug.LogWarning("  ⚠️ WARNING: No Render Pipeline assigned!");
        }
        Debug.Log("");
    }

    void CheckMaterialProperties()
    {
        Debug.Log("┌─────────────────────────────────────────────────────────┐");
        Debug.Log("│ 8. SKYBOX MATERIAL DETAILED INSPECTION                 │");
        Debug.Log("└─────────────────────────────────────────────────────────┘");
        
        if (RenderSettings.skybox != null)
        {
            Material mat = RenderSettings.skybox;
            Debug.Log($"  Material: {mat.name}");
            Debug.Log($"  Shader: {mat.shader.name}");
            Debug.Log($"  Render Queue: {mat.renderQueue}");
            Debug.Log($"  Enable Instancing: {mat.enableInstancing}");
            Debug.Log($"  Double Sided GI: {mat.doubleSidedGI}");
            
            Debug.Log("  All Properties:");
            int propCount = mat.shader.GetPropertyCount();
            for (int i = 0; i < propCount; i++)
            {
                string propName = mat.shader.GetPropertyName(i);
                var propType = mat.shader.GetPropertyType(i);
                
                Debug.Log($"    [{i}] {propName} ({propType})");
                
                try
                {
                    switch (propType)
                    {
                        case ShaderPropertyType.Color:
                            Debug.Log($"        Value: {mat.GetColor(propName)}");
                            break;
                        case ShaderPropertyType.Float:
                        case ShaderPropertyType.Range:
                            Debug.Log($"        Value: {mat.GetFloat(propName)}");
                            break;
                        case ShaderPropertyType.Texture:
                            var tex = mat.GetTexture(propName);
                            Debug.Log($"        Value: {(tex != null ? tex.name : "NULL")}");
                            break;
                        case ShaderPropertyType.Vector:
                            Debug.Log($"        Value: {mat.GetVector(propName)}");
                            break;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"        Error reading value: {e.Message}");
                }
            }
        }
        else
        {
            Debug.LogError("  ❌ ERROR: No skybox material assigned!");
        }
        Debug.Log("");
    }
}
