Shader "Custom/GradientSkybox"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (1.5, 0.5, 2.0, 1)
        _HorizonColor ("Horizon Color", Color) = (1.0, 0.3, 1.5, 1)
        _BottomColor ("Bottom Color", Color) = (0.6, 0.2, 0.9, 1)
        _Exponent ("Gradient Exponent", Float) = 2.0
        _Exposure ("Exposure", Range(0, 8)) = 2.0
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
                float3 texcoord : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 texcoord : TEXCOORD0;
            };
            
            half4 _TopColor;
            half4 _HorizonColor;
            half4 _BottomColor;
            half _Exponent;
            half _Exposure;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float3 dir = normalize(i.texcoord);
                float h = dir.y;
                
                half3 color;
                if (h > 0.0)
                {
                    float t = pow(h, _Exponent);
                    color = lerp(_HorizonColor.rgb, _TopColor.rgb, t);
                }
                else
                {
                    float t = pow(-h, _Exponent);
                    color = lerp(_HorizonColor.rgb, _BottomColor.rgb, t);
                }
                
                color *= _Exposure;
                
                return half4(color, 1.0);
            }
            ENDCG
        }
    }
    FallBack Off
}
