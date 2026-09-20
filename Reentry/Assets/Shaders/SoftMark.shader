Shader "Ember/SoftMark" {
 Properties { _BaseColor("Tint",Color)=(0,0,0,.4) }
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent"} Pass {
 Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
 HLSLPROGRAM
 #pragma vertex Vert
 #pragma fragment Frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial)
 float4 _BaseColor;
 CBUFFER_END
 struct A{float4 p:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
 V Vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;o.color=i.color;return o;}
 half4 Frag(V i):SV_Target{float2 q=i.uv*2-1;float a=saturate(1-dot(q,q));return half4(_BaseColor.rgb*i.color.rgb,_BaseColor.a*i.color.a*a*a);}
 ENDHLSL
 }}
}
