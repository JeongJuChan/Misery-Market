Shader "UI/NeonChatBubble"
{
    Properties
    {
        _BgColor    ("Background Color", Color) = (1,1,1,0.12)
        _BorderColA ("Border Color A", Color) = (0.5,0.9,1,1)
        _BorderColB ("Border Color B", Color) = (0.8,0.5,1,1)
        _GlowColor  ("Glow Color", Color) = (0.6,0.9,1,1)
        _Radius     ("Corner Radius (px)", Range(2,64)) = 16
        _BorderPx   ("Border Thickness (px)", Range(0,12)) = 2
        _GlowPx     ("Glow Width (px)", Range(0,24)) = 10
        _NoiseAmt   ("Interior Noise", Range(0,0.2)) = 0.04
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
                float4 _BgColor, _BorderColA, _BorderColB, _GlowColor;
                float  _Radius, _BorderPx, _GlowPx, _NoiseAmt;
            CBUFFER_END

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f     { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; float2 size:TEXCOORD1; };

            v2f vert(appdata v){
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                // UI Rect의 픽셀 크기 추정: _ScreenParams * UV 범위 차이를 쓰기 어려우니, 대신 uv로 상대 거리를 사용
                o.uv   = v.uv;
                // 간단히 화면 해상도로 px 근사 (정교한 px 정확도까진 불필요)
                o.size = _ScreenParams.xy;
                return o;
            }

            // SDF 라운드 사각형 (uv 0~1, px 파라미터 근사)
            float sdRoundRect(float2 uv, float2 b, float r)
            {
                // uv를 -0.5~+0.5로 정규화
                float2 p = uv * 2 - 1;
                // b: half-size(=1), r: radius in uv-space 근사
                float2 q = abs(p) - (b - r);
                return length(max(q, 0)) + min(max(q.x, q.y), 0) - r;
            }

            float4 frag(v2f i) : SV_Target
            {
                // uv 0~1 기준, px→uv 환산(대충 화면 높이에 매핑)
                float px2uv = 1.0 / max(i.size.y, 1.0);
                float r   = _Radius   * px2uv;
                float bpx = _BorderPx * px2uv;
                float gpx = _GlowPx   * px2uv;

                // 라운드 사각형 SDF
                float d = sdRoundRect(i.uv, float2(1,1), r); // 경계까지의 거리(uv)

                // 내부/테두리/외부 마스크
                float inside = step(d, 0.0);
                float border = smoothstep(bpx, 0.0, abs(d)) * inside;          // 안쪽 테두리
                float glow   = smoothstep(gpx, 0.0, abs(d));                   // 바깥쪽 부드러운 글로우

                // 보더 그라데이션(좌→우)
                float t = saturate(i.uv.x);
                float3 borderCol = lerp(_BorderColA.rgb, _BorderColB.rgb, t);

                // 배경(노이즈 살짝)
                float n = frac(sin(dot(i.uv, float2(12.9898,78.233))) * 43758.5453); // 저렴한 노이즈
                float3 bg = _BgColor.rgb + (_NoiseAmt * (n - 0.5));

                // 조합
                float3 col = bg;
                col = lerp(col, borderCol, border);                  // 테두리 색
                col += _GlowColor.rgb * glow * 0.25;                 // 부드러운 바깥 글로우

                // 알파: 배경 + 테두리로 자연스럽게
                float a = _BgColor.a * inside;
                a = max(a, border * 0.9);
                a = max(a, glow * 0.4);

                return float4(saturate(col), saturate(a));
            }
            ENDHLSL
        }
    }
}
