Shader "Ember/Plasma" {Properties{_Heat("Heat",Range(0,1))=0 _FlowTime("Flow time",Float)=0} SubShader{Tags{"Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"} Pass{Blend One OneMinusSrcAlpha ZWrite Off ZTest Always Cull Front HLSLPROGRAM
#pragma target 3.5
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
CBUFFER_START(UnityPerMaterial)
float _Heat,_FlowTime;float4 _Sources[5];
CBUFFER_END
struct A{float4 p:POSITION;};struct V{float4 p:SV_POSITION;float3 local:TEXCOORD0;};V vert(A a){V o;o.p=TransformObjectToHClip(a.p.xyz);o.local=a.p.xyz;return o;}
float hash(float3 p){return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453);}
float noise(float3 p){float3 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(lerp(hash(i),hash(i+float3(1,0,0)),f.x),lerp(hash(i+float3(0,1,0)),hash(i+float3(1,1,0)),f.x),f.y),lerp(lerp(hash(i+float3(0,0,1)),hash(i+float3(1,0,1)),f.x),lerp(hash(i+float3(0,1,1)),hash(i+1),f.x),f.y),f.z);}
half4 frag(V i):SV_Target{
 float3 ro=TransformWorldToObject(GetCameraPositionWS()),rd=normalize(i.local-ro);float3 inv=rcp(rd+1e-6);float3 a=(float3(-6,-5,-6)-ro)*inv,b=(float3(6,7,28)-ro)*inv;float3 lo=min(a,b),hi=max(a,b);float near=max(0,max(lo.x,max(lo.y,lo.z))),far=min(hi.x,min(hi.y,hi.z));
 float2 uv=GetNormalizedScreenSpaceUV(i.p);float depth=SampleSceneDepth(uv);float3 surface=ComputeWorldSpacePosition(uv,depth,UNITY_MATRIX_I_VP);far=min(far,length(surface-GetCameraPositionWS()));if(far<=near)return 0;
 float stepSize=(far-near)/32;float3 sum=0;float opacity=0;float jitter=hash(float3(floor(i.p.xy),0));
 [loop]for(int n=0;n<32;n++){
  float3 p=ro+rd*(near+(n+jitter)*stepSize);float density=0,hot=0;float turbulence=noise(p*float3(2,2,.7)-float3(0,0,_FlowTime*9));float fine=noise(p*float3(6,6,1.4)-float3(0,0,_FlowTime*18));
  [unroll]for(int j=0;j<4;j++){
   int from=j<2?0:j-1,to=j+1;float3 edge=_Sources[to].xyz-_Sources[from].xyz;float along=saturate(dot(p.xy-_Sources[from].xy,edge.xy)/max(.001,dot(edge.xy,edge.xy)));float3 q=p-lerp(_Sources[from].xyz,_Sources[to].xyz,along);float t=q.z;float trail=smoothstep(-.25,.2,t)*exp(-max(0,t)*.11);float width=.15+max(0,t)*.085;float2 bend=float2(sin(t*1.4-_FlowTime*7+j),cos(t*1.1-_FlowTime*5+j))*.055*max(0,t);float radius=length(q.xy-bend);float core=exp(-dot(q,q)*6)*.65;
   float gas=exp(-radius*radius/(width*width))*trail*_Sources[j].w;density+=gas*(.12+smoothstep(.25,.72,turbulence*.7+fine*.3)*1.5);hot+=core*2.6+gas*exp(-max(0,t)*.65);
  }
  float d=(density*.48+hot*.6)*_Heat*stepSize;float alpha=1-exp(-d);float temp=saturate(hot/(density+.001));float3 color=lerp(float3(.28,.18,.55),float3(2.5,.43,.065),smoothstep(0,.4,temp));color=lerp(color,float3(3.6,2.4,1.3),smoothstep(.35,1.1,temp));sum+=(1-opacity)*alpha*color;opacity+=(1-opacity)*alpha;
 }
 return half4(sum,opacity*.72);
}
ENDHLSL}}}
