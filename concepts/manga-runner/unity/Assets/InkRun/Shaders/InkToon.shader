// Ink Run toon shader for URP. Two steps: paper (lit) and screen-space halftone dots (shadow),
// plus an inverted-hull ink outline. Materials with "Use area accent" take their color from the
// global _InkAccent that AreaController sets, so recoloring an area is free.
// Untested in this session: written for Unity 6 URP; check the Lighting.hlsl include name on older versions.
Shader "InkRun/InkToon"
{
    Properties
    {
        _BaseColor ("Paper color", Color) = (1, 1, 1, 1)
        _InkColor ("Ink color", Color) = (0.07, 0.07, 0.07, 1)
        [Toggle] _UseAccent ("Use area accent color", Float) = 0
        _ShadowThreshold ("Shadow threshold", Range(0, 1)) = 0.45
        _DotScale ("Halftone dot cell (px)", Range(2, 24)) = 7
        _DotRadius ("Halftone dot radius", Range(0, 0.7)) = 0.33
        _OutlineWidth ("Outline width (m)", Range(0, 0.08)) = 0.018
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float4 _InkColor;
            float _UseAccent;
            float _ShadowThreshold;
            float _DotScale;
            float _DotRadius;
            float _OutlineWidth;
        CBUFFER_END
        float4 _InkAccent; // global, set by AreaController
        ENDHLSL

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionHCS : SV_POSITION; };

            Varyings vert(Attributes v)
            {
                Varyings o;
                float3 posWS = TransformObjectToWorld(v.positionOS.xyz);
                float3 nWS = normalize(TransformObjectToWorldNormal(v.normalOS));
                posWS += nWS * _OutlineWidth;
                o.positionHCS = TransformWorldToHClip(posWS);
                return o;
            }

            half4 frag(Varyings i) : SV_Target { return half4(_InkColor.rgb, 1); }
            ENDHLSL
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionHCS : SV_POSITION; float3 normalWS : TEXCOORD0; float4 screenPos : TEXCOORD1; };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.screenPos = ComputeScreenPos(o.positionHCS);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                Light mainLight = GetMainLight();
                float ndl = dot(normalize(i.normalWS), mainLight.direction) * 0.5 + 0.5;
                float3 paper = lerp(_BaseColor.rgb, _InkAccent.rgb, _UseAccent);

                float2 px = (i.screenPos.xy / i.screenPos.w) * _ScreenParams.xy / _DotScale;
                float dist = length(frac(px) - 0.5);
                float dotMask = step(dist, _DotRadius);
                float shadow = step(ndl, _ShadowThreshold);

                float3 col = lerp(paper, _InkColor.rgb, shadow * dotMask);
                return half4(col, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionHCS : SV_POSITION; };
            Varyings vert(Attributes v) { Varyings o; o.positionHCS = TransformObjectToHClip(v.positionOS.xyz); return o; }
            half4 frag(Varyings i) : SV_Target { return 0; }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
