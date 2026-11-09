Shader "Arthur/Jemaat Chapel"
{
 Properties { _MainTex("Aset karakter",2D)="white"{} _Recolor("Warna kemeja",Float)=0 }
 SubShader {
 Tags {"RenderPipeline"="UniversalPipeline" "Queue"="AlphaTest" "RenderType"="TransparentCutout"}
 Cull Off ZWrite On
 Pass {
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 struct A {float4 positionOS:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
 struct V {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
 TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
 CBUFFER_START(UnityPerMaterial)
 float4 _MainTex_ST;float _Recolor;
 CBUFFER_END
 V vert(A i){V o;o.positionCS=TransformObjectToHClip(i.positionOS.xyz);o.uv=i.uv;o.color=i.color;return o;}
 half4 frag(V i):SV_Target {half4 c=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);clip(c.a-.35);half gray=max(c.r,max(c.g,c.b));c.rgb=lerp(c.rgb,gray.xxx,_Recolor)*i.color.rgb*.8;return c;}
 ENDHLSL
 }
 }
}
