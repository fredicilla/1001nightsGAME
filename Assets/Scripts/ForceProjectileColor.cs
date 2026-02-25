using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ForceProjectileColor : MonoBehaviour
{
    [Header("Color Configuration")]
    [SerializeField] Color targetColor = Color.red;
    [SerializeField] Color emissionColor = new Color(1f, 0.2f, 0f);
    [SerializeField] bool useEmission = true;
    
    [Header("Material Properties")]
    [SerializeField] float smoothness = 0.8f;
    [SerializeField] float metallic = 0.2f;
    [SerializeField] float emissionIntensity = 2f;
    [SerializeField] bool forceOnUpdate = false;
    
    MeshRenderer meshRenderer;
    Material instanceMaterial;
    
    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        
        if (meshRenderer != null)
        {
            instanceMaterial = new Material(meshRenderer.sharedMaterial);
            meshRenderer.material = instanceMaterial;
        }
    }
    
    void Start()
    {
        ApplyColor();
    }
    
    void Update()
    {
        if (forceOnUpdate)
        {
            ApplyColor();
        }
    }
    
    void ApplyColor()
    {
        if (instanceMaterial == null) return;
        
        // Try both Lit and Unlit shader properties
        if (instanceMaterial.HasProperty("_BaseColor"))
        {
            instanceMaterial.SetColor("_BaseColor", targetColor);
            Debug.Log($"[ForceProjectileColor] Set _BaseColor to {targetColor}");
        }
        
        if (instanceMaterial.HasProperty("_Color"))
        {
            instanceMaterial.SetColor("_Color", targetColor);
            Debug.Log($"[ForceProjectileColor] Set _Color to {targetColor}");
        }
        
        // Only apply emission if using a Lit shader
        if (useEmission && instanceMaterial.shader.name.Contains("Lit"))
        {
            Color finalEmission = emissionColor * emissionIntensity;
            
            if (instanceMaterial.HasProperty("_EmissionColor"))
            {
                instanceMaterial.SetColor("_EmissionColor", finalEmission);
                Debug.Log($"[ForceProjectileColor] Set _EmissionColor to {finalEmission}");
            }
            
            instanceMaterial.EnableKeyword("_EMISSION");
            instanceMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        else
        {
            instanceMaterial.DisableKeyword("_EMISSION");
        }
        
        if (instanceMaterial.HasProperty("_Smoothness"))
        {
            instanceMaterial.SetFloat("_Smoothness", smoothness);
        }
        
        if (instanceMaterial.HasProperty("_Metallic"))
        {
            instanceMaterial.SetFloat("_Metallic", metallic);
        }
        
        if (instanceMaterial.HasProperty("_SpecularHighlights"))
        {
            instanceMaterial.SetFloat("_SpecularHighlights", 1);
        }
        
        if (instanceMaterial.HasProperty("_EnvironmentReflections"))
        {
            instanceMaterial.SetFloat("_EnvironmentReflections", 1);
        }
        
        Debug.Log($"[ForceProjectileColor] ✅ Applied color to {gameObject.name}: Shader={instanceMaterial.shader.name}, Base={targetColor}");
    }
    
    public void SetColor(Color newColor, Color newEmission)
    {
        targetColor = newColor;
        emissionColor = newEmission;
        ApplyColor();
    }
    
    void OnDestroy()
    {
        if (instanceMaterial != null)
        {
            Destroy(instanceMaterial);
        }
    }
}
