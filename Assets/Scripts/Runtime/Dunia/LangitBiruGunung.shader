Shader "Campus/LangitBiruGunung"
{
 Properties{
  _SkyTint("Langit atas",Color)=(.32,.53,.79,1)
  _HorizonColor("Cakrawala",Color)=(.62,.78,.94,1)
  _GroundColor("Bawah cakrawala",Color)=(.4,.55,.75,1)
  _MountainTint("Gunung jauh",Color)=(.32,.47,.64,1)
  _Mountains("Gunung luar",Range(0,1))=1
  _Exposure("Kecerahan",Range(0,2))=1
 }
 SubShader{
  Tags{"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" "RenderPipeline"="UniversalPipeline"}
  Cull Off ZWrite Off
  Pass{
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct A{float4 vertex:POSITION;};struct V{float4 pos:SV_POSITION;float3 dir:TEXCOORD0;};
   CBUFFER_START(UnityPerMaterial)
   half4 _SkyTint,_HorizonColor,_GroundColor,_MountainTint;float _Mountains,_Exposure;
   CBUFFER_END
   V vert(A v){V o;o.pos=TransformObjectToHClip(v.vertex.xyz);o.dir=v.vertex.xyz;return o;}
   float Hash(float n){return frac(sin(n*127.1+311.7)*43758.5453);}
   float Ridge(float a,float count,float seed){float x=frac(a)*count;float n=floor(x);return lerp(Hash(n+seed),Hash(fmod(n+1,count)+seed),frac(x));}
   float Peak(float a,float center,float width,float height){float d=abs(frac(a-center+.5)-.5);return max(0,1-d/width)*height;}
   half4 frag(V i):SV_Target{
    float3 d=normalize(i.dir);float a=atan2(d.z,d.x)*.15915494;
    half3 col=lerp(_HorizonColor.rgb,_SkyTint.rgb,smoothstep(0,.7,max(0,d.y)));
    col=lerp(col,_GroundColor.rgb,smoothstep(0,.25,-d.y));
    // Irregular peaks and short terrace steps echo the Mineways terrain.
    // Muted green foothills read as land rather than a blue ocean band.
    float summit=max(max(Peak(a,.03,.10,.24),Peak(a,.23,.105,.34)),max(Peak(a,.42,.11,.25),Peak(a,.61,.12,.31)));
    summit=max(summit,max(Peak(a,.79,.105,.28),Peak(a,.92,.08,.22)));
    float farRidge=.025+summit+Ridge(a,72,20)*.018;
    float nearRidge=.015+Ridge(a+.39,16,53)*.075+Ridge(a,64,83)*.014;
    farRidge=floor(farRidge*420)/420;nearRidge=floor(nearRidge*420)/420;
    float farMask=(1-smoothstep(farRidge-.0008,farRidge+.0008,d.y))*_Mountains;
    float nearMask=(1-smoothstep(nearRidge-.0008,nearRidge+.0008,d.y))*_Mountains;
    half3 farLand=lerp(half3(.43,.55,.48),_HorizonColor.rgb,.48);
    half3 nearLand=lerp(half3(.30,.43,.30),_HorizonColor.rgb,.24);
    float facet=floor(Ridge(a,40,15)*4)/4;
    farLand*=.94+facet*.08;nearLand*=.95+facet*.08;
    col=lerp(col,farLand,farMask);
    col=lerp(col,nearLand,nearMask);
    // Lower directions blend into subdued foothills, never a flat blue fill.
    col=lerp(col,nearLand*.88,(1-smoothstep(-.35,-.02,d.y))*_Mountains);
    return half4(col*_Exposure,1);
   }
   ENDHLSL
  }
 }
}
