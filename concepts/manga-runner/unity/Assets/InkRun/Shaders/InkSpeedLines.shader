// Full-screen radial ink speed lines for a UI RawImage (Screen Space Overlay canvas).
// _Intensity 0..1 from FX/SpeedLines.cs. Lines flicker by re-seeding a few times per second.
// Written against the built-in UI path, which URP still uses for overlay canvases. Untested in this session.
Shader "InkRun/InkSpeedLines"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _InkColor ("Ink", Color) = (0.07, 0.07, 0.07, 1)
        _Intensity ("Intensity", Range(0, 1)) = 0
        _Center ("Vanishing point (uv)", Vector) = (0.5, 0.42, 0, 0)
        _Lines ("Line count", Float) = 160
        _Flicker ("Reseeds per second", Float) = 10
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _InkColor;
            float _Intensity;
            float4 _Center;
            float _Lines;
            float _Flicker;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            float hash(float n) { return frac(sin(n) * 43758.5453); }

            v2f vert(appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o; }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 d = i.uv - _Center.xy;
                d.x *= _ScreenParams.x / _ScreenParams.y;
                float r = length(d);
                float a = (atan2(d.y, d.x) / 6.2831853) + 0.5;      // 0..1 around the circle
                float seg = floor(a * _Lines);
                float seed = seg + floor(_Time.y * _Flicker) * 131.0;
                float on = step(hash(seed), _Intensity);              // more lines as intensity rises
                float halfWidth = 0.05 + 0.16 * hash(seed * 1.7);     // in segment units
                float lineMask = step(abs(frac(a * _Lines) - 0.5), halfWidth);
                float startR = 0.10 + 0.25 * hash(seed * 3.3);
                float radial = smoothstep(startR, startR + 0.12, r);
                float alpha = on * lineMask * radial * (0.35 + 0.65 * hash(seed * 5.1)) * saturate(0.45 + _Intensity);
                return fixed4(_InkColor.rgb, alpha * _InkColor.a);
            }
            ENDCG
        }
    }
}
