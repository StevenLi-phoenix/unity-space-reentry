Shader "Ember/Sky" {Properties{_AltitudeKm("Altitude km",Float)=70 _Horizon("Horizon",Float)=.6} SubShader{Tags{"Queue"="Background" "RenderPipeline"="UniversalPipeline"}Pass{ZWrite Off ZTest Always Cull Off HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
CBUFFER_START(UnityPerMaterial)
float _AltitudeKm,_Horizon;
CBUFFER_END
struct A{float4 p:POSITION;};struct V{float4 p:SV_POSITION;};V vert(A a){V o;o.p=float4(a.p.xy,0,1);return o;}
half4 frag(V i):SV_Target{float y=i.p.y/_ScreenParams.y;

 float daylight=1-smoothstep(8,42,_AltitudeKm);float above=max(0,y-_Horizon);float rim=exp(-above*55);float3 color=lerp(float3(.0003,.0006,.002),float3(.06,.2,.39),daylight);color+=lerp(float3(.025,.13,.4),float3(.15,.22,.25),daylight)*rim;return half4(color,1);}
ENDHLSL}}}
