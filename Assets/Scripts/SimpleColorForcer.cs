using UnityEngine;

public class SimpleColorForcer : MonoBehaviour
{
    [SerializeField] Color ballColor = Color.red;
    
    void Awake()
    {
        ApplyColor();
    }
    
    void Start()
    {
        ApplyColor();
    }
    
    void ApplyColor()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            Debug.LogError($"[SimpleColorForcer] No MeshRenderer on {gameObject.name}!");
            return;
        }
        
        Material mat = new Material(renderer.sharedMaterial);
        renderer.material = mat;
        
        mat.color = ballColor;
        
        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", ballColor);
        }
        
        if (mat.HasProperty("_Color"))
        {
            mat.SetColor("_Color", ballColor);
        }
        
        if (mat.HasProperty("_EmissionColor"))
        {
            Color emissionColor = ballColor * 10f;
            mat.SetColor("_EmissionColor", emissionColor);
            mat.EnableKeyword("_EMISSION");
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            Debug.Log($"[SimpleColorForcer] ✅ Emission enabled: {emissionColor}");
        }
        
        Debug.Log($"[SimpleColorForcer] ✅ {gameObject.name} color set to {ballColor}");
    }
}
