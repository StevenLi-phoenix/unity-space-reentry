Shader "Ember/Plasma" {Properties{_BaseColor("Color",Color)=(1,.2,.03,1) _Heat("Heat",Range(0,1))=0} SubShader{Tags{"Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"} Pass{Blend SrcAlpha One ZWrite Off Cull Off HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
float4 _BaseColor;float _Heat;
struct A{float4 p:POSITION;float2 uv:TEXCOORD0;float3 n:NORMAL;};struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;float3 world:TEXCOORD1;float3 n:TEXCOORD2;};
V vert(A a){V o;float u=a.uv.x;float ripple=sin(a.uv.y*31+u*23-_Time.y*5)*sin(u*37-_Time.y*8)*u*.035;a.p.xy*=1+ripple;o.p=TransformObjectToHClip(a.p.xyz);o.world=TransformObjectToWorld(a.p.xyz);o.n=TransformObjectToWorldNormal(a.n);o.uv=a.uv;return o;}
float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
float noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
half4 frag(V i):SV_Target{float u=i.uv.x;float2 flow=float2(u*9-_Time.y*2,i.uv.y*14);float turbulence=.6*noise(flow)+.3*noise(flow*2.1)+.1*noise(flow*4.3);float edge=pow(1-abs(dot(normalize(i.n),normalize(GetCameraPositionWS()-i.world))),1.6);float fade=pow(1-u,4)*smoothstep(0,.025,u);float density=(.16+edge*.22)*smoothstep(.12,.8,turbulence)*fade*_Heat;density*=smoothstep(0,.12,1-edge);float front=exp(-u*12);float3 color=lerp(float3(1.6,.13,.025),float3(3,1.25,.3),front);color=lerp(color,float3(.35,.45,1.5),smoothstep(.45,1,u)*.5);return half4(color,density);}
ENDHLSL}}}
