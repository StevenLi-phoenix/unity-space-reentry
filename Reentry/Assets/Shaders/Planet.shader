Shader "Ember/Planet" { Properties { _BaseColor("Color",Color)=(0.02,0.12,0.22,1) } SubShader { Tags{"RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"} Pass { HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
struct A{float4 p:POSITION;float3 n:NORMAL;};struct V{float4 p:SV_POSITION;float3 n:TEXCOORD0;float3 pos:TEXCOORD1;};
V vert(A a){V o;o.p=TransformObjectToHClip(a.p.xyz);o.n=TransformObjectToWorldNormal(a.n);o.pos=a.p.xyz;return o;}
float hash(float3 p){return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453);}
float noise(float3 p){float3 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(lerp(hash(i),hash(i+float3(1,0,0)),f.x),lerp(hash(i+float3(0,1,0)),hash(i+float3(1,1,0)),f.x),f.y),lerp(lerp(hash(i+float3(0,0,1)),hash(i+float3(1,0,1)),f.x),lerp(hash(i+float3(0,1,1)),hash(i+1),f.x),f.y),f.z);}
half4 frag(V i):SV_Target{float3 p=normalize(i.pos);float n=noise(p*7)*.6+noise(p*19)*.3+noise(p*51)*.1;float land=smoothstep(.52,.57,n);float3 col=lerp(float3(.008,.065,.14),float3(.065,.16,.14),land);float cloud=smoothstep(.55,.72,noise(p*18+float3(_Time.x*.08,0,0))*.65+noise(p*43)*.35);col=lerp(col,float3(.65,.78,.86),cloud*.8);float light=saturate(dot(normalize(i.n),normalize(float3(-.4,.7,-.6))))*.85+.15;return half4(col*light,1);}
ENDHLSL } } }
