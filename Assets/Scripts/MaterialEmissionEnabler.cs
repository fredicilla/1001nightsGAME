using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class MaterialEmissionEnabler : MonoBehaviour
{
    [Header("Emission Settings")]
    [SerializeField] bool enableEmissionOnStart = true;
    [SerializeField] Color emissionColor = Color.white;
    [SerializeField] float emissionIntensity = 1f;
    
    void Start()
    {
        if (enableEmissionOnStart)
        {
            EnableEmission();
        }
    }
    
    public void EnableEmission()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer == null) return;
        
        Material material = renderer.material;
        if (material == null) return;
        
        material.EnableKeyword("_EMISSION");
        
        if (material.HasProperty("_EmissionColor"))
        {
            Color finalColor = emissionColor * emissionIntensity;
            material.SetColor("_EmissionColor", finalColor);
        }
        
        if (material.globalIlluminationFlags == MaterialGlobalIlluminationFlags.EmissiveIsBlack)
        {
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
    }
    
    public void SetEmissionColor(Color color, float intensity = 1f)
    {
        emissionColor = color;
        emissionIntensity = intensity;
        EnableEmission();
    }
}
