Shader "UI/LightSimple"
{
    Properties
    {
        [PerRendererData]_MainTex("Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)

        // UI 마스크/스텐실
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

        // 라이트
        _LightPos("Light Pos (0~1 screen UV)", Vector) = (0.5,0.5,0,0)
        _LightColor("Light Color", Color) = (1,1,1,1)
        _Ambient("Ambient", Range(0,2)) = 0.5
        _Intensity("Intensity", Range(0,10)) = 2
        _Radius("Radius", Range(0.01, 1.5)) = 0.35
        _Falloff("Falloff", Range(0.1, 8)) = 2
    }

    SubShader
    {
        Tags{
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil{
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag

            // UI 클립/알파클립
            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata{
                float4 vertex:POSITION;
                float4 color:COLOR;
                float2 uv:TEXCOORD0;
            };
            struct v2f{
                float4 pos:SV_POSITION;
                float2 uv:TEXCOORD0;
                float4 color:COLOR;
                float4 localPos:TEXCOORD1;
                float4 screenPos:TEXCOORD2;
            };

            sampler2D _MainTex; float4 _MainTex_ST;
            fixed4 _Color;
            float4 _ClipRect;

            // UI/Default가 쓰는 추가 샘플(스프라이트 아틀라스/마스크 보정용)
            fixed4 _TextureSampleAdd;

            // Light
            float4 _LightPos; fixed4 _LightColor;
            float _Ambient,_Intensity,_Radius,_Falloff;

            v2f vert(appdata v){
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                o.localPos = v.vertex;
                o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }

            fixed4 frag(v2f i):SV_Target{
                // ✅ UI 규약: 텍스처 + _TextureSampleAdd, 틴트 곱
                fixed4 baseCol = (tex2D(_MainTex, i.uv) + _TextureSampleAdd) * i.color;

                // ✅ RectMask2D/Mask
                baseCol.a *= UnityGet2DClipping(i.localPos.xy, _ClipRect);
                #ifdef UNITY_UI_ALPHACLIP
                if (baseCol.a <= 0) discard;
                #endif

                // ✅ 스크린 정규화 좌표 (0~1) — 추가 나눗셈 금지
                float2 uv01 = (i.screenPos.xy / i.screenPos.w);

                // 라이트 감쇠
                float dist   = distance(uv01, _LightPos.xy);
                float atten  = saturate(1.0 - pow(dist / max(_Radius,1e-4), _Falloff));

                // 단순 조명 모델
                float3 lit = baseCol.rgb * (_Ambient + _Intensity * atten * _LightColor.rgb);

                // ✅ 알파는 원본 유지 (불투명 방지)
                return float4(lit, baseCol.a);
            }
            ENDHLSL
        }
    }
    Fallback "UI/Default"
}