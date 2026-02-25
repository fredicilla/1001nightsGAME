Shader "Skybox/Solid Color"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0.5, 0.2, 0.8, 1)
        _BottomColor ("Bottom Color", Color) = (0.8, 0.3, 1.0, 1)
        _Intensity ("Intensity", Range(0, 5)) = 1.5
        _Exponent ("Exponent", Range(1, 8)) = 2.0
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off 
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 viewDir : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _TopColor;
                half4 _BottomColor;
                half _Intensity;
                half _Exponent;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.viewDir = input.positionOS.xyz;
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                float3 viewDir = normalize(input.viewDir);
                float t = saturate(pow(viewDir.y * 0.5 + 0.5, _Exponent));
                
                half3 color = lerp(_BottomColor.rgb, _TopColor.rgb, t);
                color *= _Intensity;
                
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
}
