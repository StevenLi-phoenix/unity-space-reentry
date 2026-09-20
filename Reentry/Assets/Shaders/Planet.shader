Shader "Ember/Planet" {Properties{_MainTex("NASA Blue Marble",2D)="white"{} _Altitude("Altitude",Float)=70} SubShader{Tags{"RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"} Pass{HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);float _Altitude;
struct A{float4 p:POSITION;float3 n:NORMAL;float2 uv:TEXCOORD0;};struct V{float4 p:SV_POSITION;float3 n:TEXCOORD0;float2 uv:TEXCOORD1;float3 w:TEXCOORD2;};V vert(A a){V o;o.p=TransformObjectToHClip(a.p.xyz);o.n=TransformObjectToWorldNormal(a.n);o.uv=a.uv;o.w=TransformObjectToWorld(a.p.xyz);return o;}
half4 frag(V i):SV_Target{float3 col=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).rgb;float3 n=normalize(i.n),view=normalize(_WorldSpaceCameraPos-i.w);float rim=pow(1-saturate(dot(n,view)),7);float light=.4+.6*saturate(dot(n,normalize(float3(-.3,1,-.4))));col*=light;float haze=saturate(length(_WorldSpaceCameraPos-i.w)/900)*.7;col=lerp(col,float3(.16,.33,.5),haze);col+=rim*float3(.04,.16,.28);return half4(col,1);}
ENDHLSL}}}
