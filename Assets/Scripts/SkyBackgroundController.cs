using UnityEngine;

public class SkyBackgroundController : MonoBehaviour
{
    [Header("Sky Texture")]
    [SerializeField] Texture2D skyTexture;
    
    [Header("Color Settings")]
    [SerializeField] Color tintColor = new Color(1.5f, 1.2f, 1.8f, 1f);
    [SerializeField] [Range(0.1f, 3f)] float brightness = 1.5f;
    
    [Header("Positioning")]
    [SerializeField] float distance = 150f;
    [SerializeField] float verticalOffset = 20f;
    [SerializeField] Vector2 scale = new Vector2(250f, 150f);
    
    private MeshRenderer meshRenderer;
    private Material skyMaterial;
    
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        
        if (meshRenderer != null && meshRenderer.sharedMaterial != null)
        {
            skyMaterial = new Material(meshRenderer.sharedMaterial);
            meshRenderer.material = skyMaterial;
            
            UpdateSkyAppearance();
        }
        
        UpdatePosition();
    }
    
    void Update()
    {
        if (skyMaterial != null)
        {
            UpdateSkyAppearance();
        }
    }
    
    void UpdateSkyAppearance()
    {
        if (skyTexture != null && skyMaterial.HasProperty("_BaseMap"))
        {
            skyMaterial.SetTexture("_BaseMap", skyTexture);
        }
        
        if (skyMaterial.HasProperty("_BaseColor"))
        {
            Color finalColor = tintColor * brightness;
            skyMaterial.SetColor("_BaseColor", finalColor);
        }
    }
    
    void UpdatePosition()
    {
        transform.localPosition = new Vector3(0, verticalOffset, distance);
        transform.localRotation = Quaternion.Euler(0, 180, 0);
        transform.localScale = new Vector3(scale.x, scale.y, 1);
    }
    
    [ContextMenu("Update Sky Texture")]
    public void UpdateSkyTexture()
    {
        if (skyMaterial != null && skyTexture != null)
        {
            skyMaterial.SetTexture("_BaseMap", skyTexture);
            Debug.Log($"Sky texture updated to: {skyTexture.name}");
        }
    }
    
    [ContextMenu("Reset to Default Position")]
    public void ResetPosition()
    {
        distance = 150f;
        verticalOffset = 20f;
        scale = new Vector2(250f, 150f);
        UpdatePosition();
    }
}
