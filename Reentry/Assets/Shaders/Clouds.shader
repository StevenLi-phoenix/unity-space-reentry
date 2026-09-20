Shader "Ember/Clouds" {SubShader{Tags{"Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"}Pass{Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
struct A{float4 p:POSITION;};struct V{float4 p:SV_POSITION;float3 pos:TEXCOORD0;float3 world:TEXCOORD1;float3 normal:TEXCOORD2;};V vert(A a){V o;o.p=TransformObjectToHClip(a.p.xyz);o.pos=a.p.xyz;o.world=TransformObjectToWorld(a.p.xyz);o.normal=TransformObjectToWorldNormal(normalize(a.p.xyz));return o;}
float hash(float3 p){return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453);}
float noise(float3 p){float3 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(lerp(hash(i),hash(i+float3(1,0,0)),f.x),lerp(hash(i+float3(0,1,0)),hash(i+float3(1,1,0)),f.x),f.y),lerp(lerp(hash(i+float3(0,0,1)),hash(i+float3(1,0,1)),f.x),lerp(hash(i+float3(0,1,1)),hash(i+1),f.x),f.y),f.z);}
half4 frag(V i):SV_Target{float3 p=normalize(i.pos)*220;float n=noise(p)*.5+noise(p*2.1)*.25+noise(p*4.1)*.15+noise(p*8.7)*.1;float a=smoothstep(.48,.65,n);float edge=noise(p*12);return half4(lerp(float3(.46,.54,.62),float3(.87,.9,.92),edge),a*.65*smoothstep(.015,.1,abs(dot(normalize(i.normal),normalize(GetCameraPositionWS()-i.world)))));}
ENDHLSL}}}
