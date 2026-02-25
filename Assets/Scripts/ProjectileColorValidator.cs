using UnityEngine;

public class ProjectileColorValidator : MonoBehaviour
{
    [Header("Color Settings")]
    [SerializeField] Color baseColor = Color.white;
    [SerializeField] Color emissionColor = Color.white;
    [SerializeField] bool enforceColorOnStart = true;
    
    [Header("Material Settings")]
    [SerializeField] float smoothness = 0.9f;
    [SerializeField] float metallic = 0.1f;
    
    const string BASE_COLOR_PROPERTY = "_BaseColor";
    const string EMISSION_COLOR_PROPERTY = "_EmissionColor";
    const string SMOOTHNESS_PROPERTY = "_Smoothness";
    const string METALLIC_PROPERTY = "_Metallic";
    
    void Start()
    {
        if (enforceColorOnStart)
        {
            ApplyColors();
        }
    }
    
    public void ApplyColors()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            Debug.LogWarning($"[ProjectileColorValidator] No MeshRenderer found on {gameObject.name}");
            return;
        }
        
        Material material = renderer.material;
        if (material == null)
        {
            Debug.LogWarning($"[ProjectileColorValidator] No material found on {gameObject.name}");
            return;
        }
        
        if (material.HasProperty(BASE_COLOR_PROPERTY))
        {
            material.SetColor(BASE_COLOR_PROPERTY, baseColor);
        }
        
        if (material.HasProperty(EMISSION_COLOR_PROPERTY))
        {
            material.SetColor(EMISSION_COLOR_PROPERTY, emissionColor);
            material.EnableKeyword("_EMISSION");
        }
        
        if (material.HasProperty(SMOOTHNESS_PROPERTY))
        {
            material.SetFloat(SMOOTHNESS_PROPERTY, smoothness);
        }
        
        if (material.HasProperty(METALLIC_PROPERTY))
        {
            material.SetFloat(METALLIC_PROPERTY, metallic);
        }
        
        Debug.Log($"[ProjectileColorValidator] Applied colors to {gameObject.name}: Base={baseColor}, Emission={emissionColor}");
    }
    
    public void SetColors(Color newBaseColor, Color newEmissionColor)
    {
        baseColor = newBaseColor;
        emissionColor = newEmissionColor;
        ApplyColors();
    }
}
