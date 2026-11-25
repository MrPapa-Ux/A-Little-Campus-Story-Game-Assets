Shader "Arthur/Crystal/Potongan Samping"
{
 Properties {
  [MainTexture] _BaseMap("Texture",2D)="white"{}
  [MainColor] _BaseColor("Color",Color)=(1,1,1,1)
  _EmissionColor("Emission",Color)=(0,0,0,0)
  _SrcBlend("Source blend",Float)=1
  _DstBlend("Destination blend",Float)=0
  _ZWrite("Depth write",Float)=1
  _Cull("Cull",Float)=2
  _CrystalGround("Keep ground surface",Float)=0
 }
 SubShader {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
  Pass {
   Tags {"LightMode"="UniversalForward"}
   Cull [_Cull] Blend [_SrcBlend] [_DstBlend] ZWrite [_ZWrite]
   HLSLPROGRAM
   #pragma target 3.0
   #pragma vertex Vert
   #pragma fragment Frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
   float4 _CrystalCutPlane;float _CrystalCutActive;float _CrystalFloorY;
   CBUFFER_START(UnityPerMaterial)
   float4 _BaseMap_ST;half4 _BaseColor;half4 _EmissionColor;float _CrystalGround;
   CBUFFER_END
   struct Attributes {float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;};
   struct Varyings {float4 positionCS:SV_POSITION;float3 positionWS:TEXCOORD0;float2 uv:TEXCOORD1;half3 light:TEXCOORD2;};
   Varyings Vert(Attributes input){
    Varyings o;o.positionWS=TransformObjectToWorld(input.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);o.uv=TRANSFORM_TEX(input.uv,_BaseMap);
    half3 normal=TransformObjectToWorldNormal(input.normalOS);half4 n=half4(normal,1);
    half3 ambient=half3(dot(unity_SHAr,n),dot(unity_SHAg,n),dot(unity_SHAb,n));half4 b=normal.xyzz*normal.yzzx;
    ambient+=half3(dot(unity_SHBr,b),dot(unity_SHBg,b),dot(unity_SHBb,b));ambient+=unity_SHC.rgb*(normal.x*normal.x-normal.y*normal.y);
    o.light=max(ambient,half3(.25h,.25h,.25h))+_MainLightColor.rgb*saturate(dot(normal,_MainLightPosition.xyz));return o;
   }
   half4 Frag(Varyings input):SV_Target{
    if(_CrystalCutActive>.5&&!(_CrystalGround>.5&&(input.positionWS.y<.04||abs(input.positionWS.y-_CrystalFloorY)<.04)))clip(dot(float4(input.positionWS,1),_CrystalCutPlane));
    half4 c=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.uv)*_BaseColor;return half4(c.rgb*input.light+_EmissionColor.rgb,c.a);
   }
   ENDHLSL
  }
 }
}
