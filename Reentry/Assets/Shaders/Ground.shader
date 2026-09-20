Shader "Ember/Ground" {SubShader{Tags{"RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"}Pass{HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
struct A{float4 p:POSITION;float3 n:NORMAL;float2 uv:TEXCOORD0;};struct V{float4 p:SV_POSITION;float3 w:TEXCOORD0;float3 n:TEXCOORD1;float2 uv:TEXCOORD2;};V vert(A a){V o;o.p=TransformObjectToHClip(a.p.xyz);o.w=TransformObjectToWorld(a.p.xyz);o.n=TransformObjectToWorldNormal(a.n);o.uv=a.uv;return o;}
float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
float noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
half4 frag(V i):SV_Target{float2 p=i.uv;float coast=p.x+12+6*sin(p.y*.027)+3*sin(p.y*.081);float dist=length(_WorldSpaceCameraPos-i.w);float3 sea=lerp(float3(.008,.065,.095),float3(.035,.22,.25),exp(-abs(coast)*.5));float2 plots=float2(p.x*3.5,p.y*1.8)+float2(noise(p*.3),noise(p*.2))*.7;float farmland=hash(floor(plots));float grain=noise(p*80);float3 field=lerp(float3(.11,.16,.07),float3(.31,.27,.12),farmland);float forests=smoothstep(.44,.69,noise(p*.42));field=lerp(field,float3(.035,.1,.065),forests);field*=.84+grain*.24;
 float2 cell=abs(frac(plots)-.5);float lanes=smoothstep(.485,.498,max(cell.x,cell.y));field=lerp(field,float3(.33,.32,.23),lanes*.45);
 float highway=abs(p.x-(3.5+sin(p.y*.12)*.6));field=lerp(field,float3(.14,.14,.13),(1-smoothstep(.018,.026,highway)));
 float3 land=lerp(float3(.52,.45,.28),field,smoothstep(0,.35,coast));float3 col=lerp(sea,land,smoothstep(-.08,.06,coast));float light=.4+.6*saturate(dot(normalize(i.n),normalize(float3(-.3,1,-.4))));col*=light;
 float foam=exp(-abs(coast)*28)*(.45+.4*sin(p.y*45));col+=foam*.23;float haze=1-exp(-dist*.009);col=lerp(col,float3(.23,.37,.47),haze*.88);return half4(col,1);}
ENDHLSL}}}
