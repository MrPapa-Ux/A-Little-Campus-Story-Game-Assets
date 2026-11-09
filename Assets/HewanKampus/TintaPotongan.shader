Shader "Campus/TintaPotongan"
{
 Properties { [PerRendererData] _MainTex("Atlas",2D)="white"{} _Mode("Mode",Float)=0 _Rect("Sprite Rect",Vector)=(0,0,1,1) }
 SubShader {
 Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
 Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 Pass { HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);float4 _MainTex_TexelSize;float _Mode;float4 _Rect;
 struct A{float4 position:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
 struct V{float4 position:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
 V vert(A a){V o;o.position=TransformObjectToHClip(a.position.xyz);o.uv=a.uv;o.color=a.color;return o;}
 half4 frag(V i):SV_Target{
  half4 c=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);float2 uv=(i.uv-_Rect.xy)/max(_Rect.zw,float2(.0001,.0001));
  if(_Mode>.5){float2 d=_MainTex_TexelSize.xy*1.7;float edge=min(min(SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv+float2(d.x,0)).a,SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv-float2(d.x,0)).a),min(SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv+float2(0,d.y)).a,SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv-float2(0,d.y)).a));
   half3 fill=c.rgb;if(_Mode<1.5)fill=uv.y>.9?half3(.96,.76,.59):half3(0,.81,.8);
   else if(_Mode<2.5)fill=half3(.055,.045,.035);
   else if(_Mode<3.5)fill=uv.y<.27?half3(.96,.96,.94):half3(.04,.18,.78);
   else fill=half3(.96,.76,.59);
   c.rgb=lerp(c.rgb,fill,smoothstep(.1,.8,edge));
  }return c*i.color;
 }
 ENDHLSL }
 }
}
