using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SkyboxGradientOverlay : MonoBehaviour
{
    [Header("Gradient Colors")]
    [SerializeField] Color topColor = new Color(0.8f, 0.3f, 1.0f);
    [SerializeField] Color bottomColor = new Color(0.4f, 0.15f, 0.6f);
    
    [Header("Settings")]
    [SerializeField] [Range(0.1f, 3f)] float gradientIntensity = 1.5f;
    
    private Material gradientMaterial;
    
    void Start()
    {
        Shader shader = Shader.Find("Hidden/SkyboxGradient");
        if (shader == null)
        {
            shader = CreateGradientShader();
        }
        
        if (shader != null)
        {
            gradientMaterial = new Material(shader);
            UpdateMaterial();
        }
    }
    
    void Update()
    {
        if (gradientMaterial != null)
        {
            UpdateMaterial();
        }
    }
    
    void UpdateMaterial()
    {
        gradientMaterial.SetColor("_TopColor", topColor * gradientIntensity);
        gradientMaterial.SetColor("_BottomColor", bottomColor * gradientIntensity);
    }
    
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (gradientMaterial != null)
        {
            Graphics.Blit(src, dest, gradientMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
    
    Shader CreateGradientShader()
    {
        string shaderCode = @"
        Shader ""Hidden/SkyboxGradient""
        {
            Properties
            {
                _MainTex (""Texture"", 2D) = ""white"" {}
                _TopColor (""Top Color"", Color) = (0.8, 0.3, 1, 1)
                _BottomColor (""Bottom Color"", Color) = (0.4, 0.15, 0.6, 1)
            }
            
            SubShader
            {
                Tags { ""RenderType""=""Opaque"" }
                
                Pass
                {
                    CGPROGRAM
                    #pragma vertex vert
                    #pragma fragment frag
                    #include ""UnityCG.cginc""
                    
                    struct appdata
                    {
                        float4 vertex : POSITION;
                        float2 uv : TEXCOORD0;
                    };
                    
                    struct v2f
                    {
                        float2 uv : TEXCOORD0;
                        float4 vertex : SV_POSITION;
                    };
                    
                    sampler2D _MainTex;
                    float4 _TopColor;
                    float4 _BottomColor;
                    
                    v2f vert (appdata v)
                    {
                        v2f o;
                        o.vertex = UnityObjectToClipPos(v.vertex);
                        o.uv = v.uv;
                        return o;
                    }
                    
                    fixed4 frag (v2f i) : SV_Target
                    {
                        fixed4 col = tex2D(_MainTex, i.uv);
                        float t = i.uv.y;
                        fixed4 gradient = lerp(_BottomColor, _TopColor, t);
                        return col * 0.3 + gradient * 0.7;
                    }
                    ENDCG
                }
            }
        }";
        
        return Shader.Find("Hidden/SkyboxGradient");
    }
}
