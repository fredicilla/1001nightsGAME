Shader "Custom/GradientSkybox"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0.5, 0.3, 0.9, 1)
        _HorizonColor ("Horizon Color", Color) = (0.8, 0.4, 0.7, 1)
        _BottomColor ("Bottom Color", Color) = (0.95, 0.87, 0.75, 1)
        _Exponent ("Gradient Exponent", Float) = 2.0
    }
    
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 uv : TEXCOORD0;
            };
            
            float4 _TopColor;
            float4 _HorizonColor;
            float4 _BottomColor;
            float _Exponent;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float3 dir = normalize(i.uv);
                float h = dir.y;
                
                fixed4 color;
                if (h > 0.0)
                {
                    // Top to Horizon
                    float t = pow(h, _Exponent);
                    color = lerp(_HorizonColor, _TopColor, t);
                }
                else
                {
                    // Horizon to Bottom
                    float t = pow(-h, _Exponent);
                    color = lerp(_HorizonColor, _BottomColor, t);
                }
                
                return color;
            }
            ENDCG
        }
    }
    FallBack Off
}
