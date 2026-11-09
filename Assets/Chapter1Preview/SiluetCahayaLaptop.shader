Shader "Campus/Pratinjau/SiluetCahayaLaptop" {
 Properties {[PerRendererData] _MainTex("Sprite",2D)="white"{}}
 SubShader {
 Tags {"Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"}
 Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 Pass {HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);float4 _MainTex_TexelSize;
 struct A{float4 p:POSITION;float2 uv:TEXCOORD0;float4 c:COLOR;};
 struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;float4 c:COLOR;};
 V vert(A a){V o;o.p=TransformObjectToHClip(a.p.xyz);o.uv=a.uv;o.c=a.c;return o;}
 half4 frag(V i):SV_Target {
 half4 c=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
 float2 d=_MainTex_TexelSize.xy*1.5;
 half edge=1-min(SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv+float2(d.x,0)).a,SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv+float2(0,d.y)).a);
 c.rgb=c.rgb*i.c.rgb+half3(.009,.016,.021)+edge*half3(.025,.06,.075);
 c.a*=i.c.a;return c;}
 ENDHLSL}
 }
}
