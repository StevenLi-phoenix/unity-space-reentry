Shader "Ember/Atmosphere" {Properties{_BaseColor("Color",Color)=(.05,.4,1,1)} SubShader{Tags{"Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"} Pass{Blend SrcAlpha One ZWrite Off Cull Back HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
float4 _BaseColor;struct A{float4 p:POSITION;float3 n:NORMAL;};struct V{float4 p:SV_POSITION;float3 n:TEXCOORD0;float3 w:TEXCOORD1;};V vert(A a){V o;o.p=TransformObjectToHClip(a.p.xyz);o.n=TransformObjectToWorldNormal(a.n);o.w=TransformObjectToWorld(a.p.xyz);return o;}half4 frag(V i):SV_Target{float rim=pow(1-saturate(dot(normalize(i.n),normalize(_WorldSpaceCameraPos-i.w))),4);return half4(_BaseColor.rgb,rim*.48);}
ENDHLSL}}}
