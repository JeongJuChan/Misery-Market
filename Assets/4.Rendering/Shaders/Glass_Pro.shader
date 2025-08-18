Shader "UI/Glass_Pro"
{
    Properties
    {
        _GlassColor     ("Glass Color (RGBA)", Color) = (0.05,0.05,0.06,0.66)
        _GradTopColor   ("Gradient Top", Color) = (0.05,0.05,0.06,1)
        _GradBotColor   ("Gradient Bottom", Color) = (0.24,0.12,0.10,1)
        _GradStrength   ("Gradient Strength", Range(0,1)) = 0.3

        [Toggle]_UseSceneColor ("Use Scene Color (OpaqueTex)", Float) = 1
        _RefractionStrength ("Refraction Strength", Range(0,0.05)) = 0.02
        _BlurRadius     ("Blur Radius (px)", Range(0,8)) = 3.0

        _NormalTex      ("Normal/Noise (RG)", 2D) = "bump" {}
        _NormalScale    ("Normal Tiling", Float) = 2.0
        _NormalStrength ("Normal Strength", Range(0,1)) = 0.6

        _FresnelColor   ("Fresnel Color", Color) = (0.88,0.28,0.18,1)
        _FresnelPower   ("Fresnel Power", Range(1,8)) = 3.0
        _FresnelIntensity ("Fresnel Intensity", Range(0,2)) = 0.35

        _EnvTex         ("Env Cubemap (optional)", CUBE) = "" {}
        _SpecIntensity  ("Spec Intensity", Range(0,1)) = 0.12

        _Radius         ("Corner Radius (px)", Range(0,64)) = 20
        _BorderPx       ("Border (px)", Range(0,8)) = 2
        _BorderColor    ("Border Color", Color) = (1,1,1,0.14)
        _VignetteTop    ("Vignette Top", Range(0,1)) = 0.75
        _VignetteBottom ("Vignette Bottom", Range(0,1)) = 0.85
        _NoiseAmt       ("Grain Amount", Range(0,0.1)) = 0.035
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

            // URP OpaqueTexture (씬 컬러)
            TEXTURE2D_X(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
            TEXTURECUBE(_EnvTex);  SAMPLER(sampler_EnvTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _GlassColor;
                float4 _GradTopColor, _GradBotColor;
                float  _GradStrength;

                float  _UseSceneColor;
                float  _RefractionStrength;
                float  _BlurRadius;

                float4 _NormalTex_ST;
                float  _NormalScale;
                float  _NormalStrength;

                float4 _FresnelColor;
                float  _FresnelPower;
                float  _FresnelIntensity;

                float  _SpecIntensity;

                float  _Radius, _BorderPx;
                float4 _BorderColor;
                float  _VignetteTop, _VignetteBottom;

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

            float sdRoundRect(float2 uv, float2 halfSize, float radius)
            {
                float2 p = uv * 2.0 - 1.0;
                float2 q = abs(p) - (halfSize - radius);
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - radius;
            }

            float rand(float2 p){ return frac(sin(dot(p, float2(12.9898,78.233))) * 43758.5453); }

            // ❗전역 _CameraOpaqueTexture를 직접 샘플하는 3x3 블러
            float3 Blur9(float2 uv, float2 texel)
            {
                const float k[9] = {1,2,1, 2,4,2, 1,2,1};
                float3 acc = 0;
                float  wsum = 0;
                int idx = 0;
                [unroll] for(int y=-1; y<=1; y++){
                    [unroll] for(int x=-1; x<=1; x++){
                        float w = k[idx++];
                        float2 uv2 = uv + float2(x,y)*texel;
                        float3 c = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv2).rgb;
                        acc += w * c;
                        wsum += w;
                    }
                }
                return acc / max(wsum, 1e-4);
            }

            float4 frag(v2f i) : SV_Target
            {
                float px2uv = 1.0 / max(i.screenSize.y, 1.0);

                // 라운드 마스크/보더
                float d = sdRoundRect(i.uv, float2(1.0,1.0), _Radius * px2uv);
                float inside = step(d, 0.0);
                float borderMask = smoothstep(_BorderPx * px2uv, 0.0, abs(d)) * inside;

                // 화면 UV (씬 컬러용)
                float2 suv = (i.pos.xy / _ScreenParams.xy);
            #if UNITY_UV_STARTS_AT_TOP
                suv.y = 1.0 - suv.y;
            #endif

                // 노멀(2D → XY)
                float2 nUV = i.uv * _NormalScale;
                float2 n2 = SAMPLE_TEXTURE2D(_NormalTex, sampler_NormalTex, TRANSFORM_TEX(nUV, _NormalTex)).rg * 2.0 - 1.0;
                n2 *= _NormalStrength;

                // 기본 컬러 (폴백)
                float3 baseCol = _GlassColor.rgb;

                // 씬 컬러 굴절 + 블러
                if (_UseSceneColor > 0.5)
                {
                    float2 refrUV = suv + n2 * _RefractionStrength;
                    float2 texel = _BlurRadius * px2uv;
                    float3 blurred = Blur9(refrUV, texel);
                    baseCol = blurred;
                }

                // 수직 그라데이션 틴트
                float3 grad = lerp(_GradTopColor.rgb, _GradBotColor.rgb, saturate(i.uv.y));
                baseCol = lerp(baseCol, grad, _GradStrength);

                // 비네트
                float vTop    = smoothstep(0.0, 1.0, i.uv.y);
                float vBottom = 1.0 - vTop;
                float vWeight = saturate(1.0 - (_VignetteTop * vTop + _VignetteBottom * vBottom) * 0.5);
                baseCol *= vWeight;

                // 프레넬 가장자리
                float edgeMask = smoothstep(3.0*px2uv, 0.0, abs(d)) * inside;
                float3 N = normalize(float3(n2, sqrt(saturate(1.0 - dot(n2,n2)))));
                float3 V = float3(0,0,1);
                float fres = pow(saturate(1.0 - dot(N, V)), _FresnelPower);
                baseCol += _FresnelColor.rgb * (_FresnelIntensity * fres * edgeMask);

                // 큐브맵 반사(옵션)
                if (_SpecIntensity > 0.001)
                {
                    float3 R = reflect(-V, N);
                    float3 env = SAMPLE_TEXTURECUBE(_EnvTex, sampler_EnvTex, R).rgb;
                    baseCol += env * _SpecIntensity * edgeMask;
                }

                // 그레인
                baseCol += (rand(i.uv * 512.0) - 0.5) * _NoiseAmt;

                // 보더
                baseCol = lerp(baseCol, _BorderColor.rgb, borderMask);

                // 알파
                float a = _GlassColor.a * inside;
                a = max(a, borderMask * _BorderColor.a);

                return float4(saturate(baseCol), saturate(a));
            }
            ENDHLSL
        }
    }
}
