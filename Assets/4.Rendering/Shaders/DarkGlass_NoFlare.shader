Shader "UI/DarkGlass_NoFlare"
{
    Properties
    {
        // 유리색 (알파로 투명도 제어)
        _GlassColor ("Glass Color (RGBA)", Color) = (0.08, 0.10, 0.14, 0.60)

        // 상/하단 비네팅(어둡게)
        _VignetteTop    ("Vignette Top (0~1)", Range(0,1)) = 0.7
        _VignetteBottom ("Vignette Bottom (0~1)", Range(0,1)) = 0.7

        // 라운드 사각형 모서리/보더
        _Radius         ("Corner Radius (px)", Range(0,64)) = 20
        _BorderPx       ("Border (px)", Range(0,8)) = 2
        _BorderColor    ("Border Color", Color) = (1,1,1,0.12)

        // 미세 노이즈
        _NoiseAmt       ("Noise Amount", Range(0,0.1)) = 0.02
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _GlassColor;
                float  _VignetteTop, _VignetteBottom;
                float  _Radius, _BorderPx;
                float4 _BorderColor;
                float  _NoiseAmt;
            CBUFFER_END

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; float2 screenSize:TEXCOORD1; };

            v2f vert(appdata v){
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv  = v.uv;
                o.screenSize = _ScreenParams.xy;
                return o;
            }

            // 라운드 사각형 SDF
            float sdRoundRect(float2 uv, float2 halfSize, float radius)
            {
                float2 p = uv * 2.0 - 1.0;
                float2 q = abs(p) - (halfSize - radius);
                return length(max(q,0.0)) + min(max(q.x, q.y), 0.0) - radius;
            }

            float rand(float2 p){ return frac(sin(dot(p, float2(12.9898,78.233))) * 43758.5453); }

            float4 frag(v2f i) : SV_Target
            {
                float px2uv = 1.0 / max(i.screenSize.y, 1.0);

                // 라운드 사각형 마스크
                float d = sdRoundRect(i.uv, float2(1.0,1.0), _Radius * px2uv);
                float inside = step(d, 0.0);
                float borderMask = smoothstep(_BorderPx * px2uv, 0.0, abs(d)) * inside;

                // 기본 유리색 + 위아래 비네트
                float vTop    = smoothstep(0.0, 1.0, i.uv.y);
                float vBottom = 1.0 - vTop;
                float vWeight = saturate(1.0 - (_VignetteTop * vTop + _VignetteBottom * vBottom) * 0.5);

                float3 col = _GlassColor.rgb * vWeight;

                // 미세 노이즈
                col += (rand(i.uv * 512.0) - 0.5) * _NoiseAmt;

                // 보더 적용
                col = lerp(col, _BorderColor.rgb, borderMask);

                // 알파
                float a = _GlassColor.a * inside;
                a = max(a, borderMask * _BorderColor.a);

                return float4(saturate(col), saturate(a));
            }
            ENDHLSL
        }
    }
}
