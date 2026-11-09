Shader "Campus/BayanganHewan"
{
 Properties{_Color("Warna",Color)=(0,0,0,.16)}
 SubShader{
  Tags{"RenderPipeline"="UniversalPipeline" "Queue"="Transparent-10" "RenderType"="Transparent"}
  Pass{
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off
   Cull Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct A{float4 positionOS:POSITION;float2 uv:TEXCOORD0;};
   struct V{float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;};
   CBUFFER_START(UnityPerMaterial)
   float4 _Color;
   CBUFFER_END
   V vert(A i){V o;o.positionCS=TransformObjectToHClip(i.positionOS.xyz);o.uv=i.uv;return o;}
   half4 frag(V i):SV_Target{float d=length((i.uv-.5)*2);return half4(_Color.rgb,_Color.a*(1-smoothstep(.25,1,d)));}
   ENDHLSL
  }
 }
}
