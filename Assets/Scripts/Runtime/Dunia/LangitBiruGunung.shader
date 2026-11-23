Shader "Campus/LangitBiruGunung"
{
 Properties{
  _SkyTint("Langit atas",Color)=(.32,.53,.79,1)
  _HorizonColor("Cakrawala",Color)=(.62,.78,.94,1)
  _GroundColor("Bawah cakrawala",Color)=(.4,.55,.75,1)
  _MountainTint("Gunung jauh",Color)=(.32,.47,.64,1)
  _Mountains("Gunung luar",Range(0,1))=1
  _Exposure("Kecerahan",Range(0,2))=1
  _Bulan("Bulan malam",Range(0,1))=0
  _Awan("Kepadatan awan hujan",Range(0,1))=0
  _ArahBulan("Arah bulan",Vector)=(.242,.303,.922,0)
  _Matahari("Matahari siang",Range(0,1))=1
  _ArahMatahari("Arah matahari",Vector)=(-.4,.5,.77,0)
  _WarnaMatahari("Warna matahari",Color)=(1,.91,.57,1)
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
   half4 _SkyTint,_HorizonColor,_GroundColor,_MountainTint;float _Mountains,_Exposure,_Bulan,_Awan;
   float4 _ArahBulan,_ArahMatahari; half4 _WarnaMatahari; float _Matahari;
   CBUFFER_END
   V vert(A v){V o;o.pos=TransformObjectToHClip(v.vertex.xyz);o.dir=v.vertex.xyz;return o;}
   float Hash(float n){return frac(sin(n*127.1+311.7)*43758.5453);}
   float AwanNoise(float2 p){
    float2 cell=floor(p),f=frac(p);f=f*f*(3-2*f);float n=dot(cell,float2(17,59));
    return lerp(lerp(Hash(n),Hash(n+17),f.x),lerp(Hash(n+59),Hash(n+76),f.x),f.y);
   }
   float Ridge(float a,float count,float seed){float x=frac(a)*count;float n=floor(x);return lerp(Hash(n+seed),Hash(fmod(n+1,count)+seed),frac(x));}
   float Peak(float a,float center,float width,float height){float d=abs(frac(a-center+.5)-.5);return max(0,1-d/width)*height;}
   half4 frag(V i):SV_Target{
    float3 d=normalize(i.dir);float a=atan2(d.z,d.x)*.15915494;
    half3 col=lerp(_HorizonColor.rgb,_SkyTint.rgb,smoothstep(0,.7,max(0,d.y)));
    col=lerp(col,_GroundColor.rgb,smoothstep(0,.25,-d.y));
    // A small pencil-grained disc, drawn behind clouds and terrain.
    float jarakSurya=dot(d,normalize(_ArahMatahari.xyz));
    float garis=sin(d.x*1800+d.y*970)*sin(d.y*1400-d.z*730);
    float cakramSurya=smoothstep(.99855,.99875,jarakSurya+garis*.000025);
    float pijarSurya=pow(saturate((jarakSurya-.985)/.015),3)*.22;
    col=lerp(col,_WarnaMatahari.rgb*(.97+garis*.025),max(cakramSurya,pijarSurya)*_Matahari);
    // Fair-weather clouds remain present when the separate rain blanket is clear.
    float2 bidangAwan=d.xz/max(.12,d.y)*1.7+float2(_Time.y*.003,_Time.y*.001);
    float gumpal=AwanNoise(bidangAwan)*.65+AwanNoise(bidangAwan*2.1)*.25+AwanNoise(bidangAwan*4.3)*.10;
    float tepi=smoothstep(.54,.70,gumpal+garis*.007);
    float awanCerah=tepi*smoothstep(.04,.18,d.y)*(1-smoothstep(.85,1,d.y))*_Mountains*(1-_Awan);
    half3 putihAwan=lerp(_HorizonColor.rgb*1.23,_SkyTint.rgb*.85,saturate((.67-gumpal)*2));
    col=lerp(col,putihAwan,awanCerah*.88);
    if (_Awan > .001) {
     // Lapisan awan menyatu pada skybox: tanpa partikel, transparansi, atau draw call tambahan.
     float gerak=_Time.y*.012;
     float lipatan=sin(a*75+gerak)*.15+sin(a*153-gerak*.7)*.07;
     float bentuk=sin(d.y*17+lipatan*5+sin(a*31+gerak)*1.2)*.5+.5;
     float selimut=smoothstep(1-_Awan*1.35,1.12-_Awan*.8,bentuk);
     selimut=max(selimut,_Awan*.72)*smoothstep(-.02,.12,d.y);
     half3 awanGelap=_SkyTint.rgb*.35+half3(.025,.03,.035);
     half3 awanTerang=_HorizonColor.rgb*1.45+half3(.035,.04,.045);
     col=lerp(col,lerp(awanGelap,awanTerang,bentuk*.8),saturate(selimut)*_Awan);
    }
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
    col *= _Exposure;
    if (_Bulan > .001) {
     float jarak = dot(d, _ArahBulan.xyz);
     float cakram = smoothstep(.99960,.99972,jarak);
     float halo = smoothstep(.995,.9997,jarak)*.16;
     half3 warnaBulan = half3(.95,.94,.78) - half3(.10,.09,.04) *
        smoothstep(0,.004,abs(d.x-_ArahBulan.x-.006)+abs(d.y-_ArahBulan.y-.006)*.5);
     col = lerp(col, warnaBulan, _Bulan * max(cakram,halo));
    }
    return half4(col,1);
   }
   ENDHLSL
  }
 }
}
