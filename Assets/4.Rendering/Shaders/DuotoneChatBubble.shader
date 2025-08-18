Shader "UI/DuotoneChatBubble"
{
    Properties
    {
        _ColorA     ("Duotone Color A", Color) = (0.62, 0.5, 1.0, 1)
        _ColorB     ("Duotone Color B", Color) = (0.0, 0.9, 0.8, 1)
        _Opacity    ("Opacity", Range(0,1)) = 0.85

        [Toggle]_UseMainTex ("Use MainTex Luma", Float) = 0
        [MainTexture]_MainTex ("MainTex (optional)", 2D) = "white" {}

        _AngleDeg   ("Gradient Angle (deg)", Range(0,360)) = 90
        _Contrast   ("Contrast", Range(0,2)) = 1
        _Brightness ("Brightness", Range(-1,1)) = 0
        _Saturation ("Saturation", Range(0,2)) = 1

        _Radius     ("Corner Radius (px)", Range(2,64)) = 18
        _BorderPx   ("Border Thickness (px)", Range(0,8)) = 2
        _BorderCol  ("Border Color", Color) = (1,1,1,0.35)
        _NoiseAmt   ("Subtle Noise", Range(0,0.2)) = 0.03
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
                float4 _ColorA, _ColorB;
                float  _Opacity;
                float  _UseMainTex;
                float4 _MainTex_ST;
                float  _AngleDeg;
                float  _Contrast, _Brightness, _Saturation;
                float  _Radius, _BorderPx;
                float4 _BorderCol;
                float  _NoiseAmt;
            CBUFFER_END

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; float2 screenSize:TEXCOORD1; };

            v2f vert(appdata v){
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv  = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenSize = _ScreenParams.xy;
                return o;
            }

            // --- SDF round-rect (uv 0~1) ---
            float sdRoundRect(float2 uv, float2 halfSize, float radius)
            {
                // halfSize는 여기서 (1,1)로 쓰므로 사실상 정규화 영역
                float2 p = uv * 2.0 - 1.0;
                float2 q = abs(p) - (halfSize - radius);
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - radius;
            }

            float rand(float2 p){ return frac(sin(dot(p,float2(12.9898,78.233)))*43758.5453); }

            float3 ApplySaturation(float3 c, float s){
                float l = dot(c, float3(0.299,0.587,0.114));
                return lerp(l.xxx, c, s);
            }

            float4 frag(v2f i) : SV_Target
            {
                // px → uv 환산(화면 높이 기준)
                float px2uv = 1.0 / max(i.screenSize.y, 1.0);

                // 라운드 버블 SDF (halfSize = (1,1))
                float d = sdRoundRect(i.uv, float2(1.0, 1.0), _Radius * px2uv);
                float inside = step(d, 0.0);

                // 듀오톤 보간 인자 t (0~1)
                float t;
                if (_UseMainTex > 0.5)
                {
                    float3 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).rgb;
                    t = dot(tex, float3(0.299, 0.587, 0.114)); // luma
                }
                else
                {
                    float2 p = i.uv - 0.5;
                    float ang = radians(_AngleDeg);
                    float s = sin(ang), c = cos(ang);
                    float2 r = float2( c*p.x - s*p.y, s*p.x + c*p.y );
                    t = saturate(r.y + 0.5);
                    t = smoothstep(0.0, 1.0, t);
                }

                // 톤 보정
                t = (t - 0.5) * _Contrast + 0.5 + _Brightness;
                t = saturate(t);

                // 듀오톤 색
                float3 col = lerp(_ColorA.rgb, _ColorB.rgb, t);
                col = ApplySaturation(col, _Saturation);

                // 미세 노이즈
                col += (rand(i.uv*512.0) - 0.5) * _NoiseAmt;

                // 테두리
                float border = smoothstep(_BorderPx * px2uv, 0.0, abs(d)) * inside;
                col = lerp(col, _BorderCol.rgb, border);

                // 알파
                float a = _Opacity * inside;
                a = max(a, border * _BorderCol.a);

                return float4(saturate(col), saturate(a));
            }
            ENDHLSL
        }
    }
}
