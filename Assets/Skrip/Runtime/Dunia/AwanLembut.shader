Shader "Arthur/AwanLembut"
{
    Properties { _Color("Warna", Color) = (.94,.97,1,.65) _Seed("Bentuk", Float)=0 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct A { float4 positionOS:POSITION; float2 uv:TEXCOORD0; };
            struct V { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; };
            CBUFFER_START(UnityPerMaterial)
            float4 _Color; float _Seed;
            CBUFFER_END
            V vert(A i){ V o; o.positionCS=TransformObjectToHClip(i.positionOS.xyz);o.uv=i.uv;return o; }
            float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
            half4 frag(V i):SV_Target
            {
                float2 p=(i.uv-.5)*float2(2.6,2.0);
                float shape=0;
                for(int k=0;k<5;k++){
                    float2 c=float2(-.78+k*.38,.07+.17*sin(k*2.4+_Seed));
                    float2 d=(p-c)/float2(.58,.46+.12*sin(k+_Seed));
                    shape=max(shape,1-length(d));
                }
                float n=noise(i.uv*9+_Seed)*.6+noise(i.uv*20+_Seed)*.25;
                float alpha=smoothstep(.01,.25,shape+(n-.43)*.22)*_Color.a;
                float shade=.91+.09*smoothstep(.15,.8,i.uv.y);
                return half4(_Color.rgb*shade,alpha);
            }
            ENDHLSL
        }
    }
}
