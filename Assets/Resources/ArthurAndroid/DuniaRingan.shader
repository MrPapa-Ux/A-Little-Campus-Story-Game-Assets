Shader "Arthur/Android/Dunia Ringan"
{
    Properties
    {
        [MainTexture] _BaseMap("Texture", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        _Cutoff("Alpha cutoff", Range(0,1)) = 0.5
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2
        _EmissionColor("Emission", Color) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Cull [_Cull]
            ZWrite On
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_local_fragment _ _ALPHATEST_ON
            // Hanya binding yang diperlukan; jangan membawa sampler pantulan/cookie
            // dari Lighting.hlsl pada driver GLES ponsel lama.
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _EmissionColor;
                half _Cutoff;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; half3 light:TEXCOORD1; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                half3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                half4 n = half4(normalWS, 1);
                half3 ambient = half3(dot(unity_SHAr,n),dot(unity_SHAg,n),dot(unity_SHAb,n));
                half4 b = normalWS.xyzz * normalWS.yzzx;
                ambient += half3(dot(unity_SHBr,b),dot(unity_SHBg,b),dot(unity_SHBb,b));
                ambient += unity_SHC.rgb * (normalWS.x * normalWS.x - normalWS.y * normalWS.y);
                output.light = max(ambient, half3(.18h,.18h,.18h))
                    + _MainLightColor.rgb * saturate(dot(normalWS, _MainLightPosition.xyz));
                return output;
            }
            half4 Frag(Varyings input):SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.uv) * _BaseColor;
                #if defined(_ALPHATEST_ON)
                    clip(color.a - _Cutoff);
                #endif
                return half4(color.rgb * input.light + _EmissionColor.rgb, 1);
            }
            ENDHLSL
        }
    }
}
