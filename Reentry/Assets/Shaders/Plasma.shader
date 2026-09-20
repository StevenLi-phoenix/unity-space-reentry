Shader "Ember/Plasma" {Properties{_BaseColor("Color",Color)=(1,.2,.03,1)} SubShader{Tags{"Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"} Pass{Blend SrcAlpha One ZWrite Off Cull Off HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
float4 _BaseColor;struct A{float4 p:POSITION;float2 uv:TEXCOORD0;float4 col:COLOR;};struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;float4 col:COLOR;};V vert(A a){V o;o.p=TransformObjectToHClip(a.p.xyz);o.uv=a.uv;o.col=a.col;return o;}half4 frag(V i):SV_Target{float a=pow(saturate(1-abs(i.uv.y*2-1)),2)*(1-i.uv.x);float pulse=.7+.3*sin(i.uv.x*45-_Time.y*18);return half4(_BaseColor.rgb*i.col.rgb*2,a*pulse*_BaseColor.a*i.col.a);}
ENDHLSL}}}
