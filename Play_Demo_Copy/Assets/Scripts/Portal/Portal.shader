// ============================================================
// 传送门材质 Shader（URP）—— 蓝色半透明能量门
// 屏幕空间采样：门内相机把墙对面渲染进 RenderTexture，这里按屏幕位置采样。
// ============================================================
Shader "Custom/Portal"
{
    Properties
    {
        _MainTex      ("门内画面 (Render Texture)", 2D) = "white" {}
        _TintColor    ("蓝色色调", Color) = (0.10, 0.50, 1.00, 1.00)
        _TintStrength ("蓝色浓度", Range(0.0, 1.0)) = 0.5
        _Alpha        ("整体透明度", Range(0.0, 1.0)) = 0.8
        _Brightness   ("画面亮度", Range(0.0, 3.0)) = 1.2
        _RimColor     ("光环颜色", Color) = (0.30, 0.85, 1.00, 1.00)
        _RimPower     ("光环强度", Range(0.0, 10.0)) = 3.5
        _RimWidth     ("光环宽度", Range(0.0, 0.5)) = 0.15
        _EdgeSoftness ("边缘柔化", Range(0.0, 0.5)) = 0.12
        [Toggle] _FlipY ("画面垂直翻转 (颠倒时勾选)", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "PortalPass"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _TintColor;
                half4 _RimColor;
                half  _TintStrength;
                half  _Alpha;
                half  _Brightness;
                half  _RimPower;
                half  _RimWidth;
                half  _EdgeSoftness;
                half  _FlipY;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 screenPos   : TEXCOORD0;
                float2 uv          : TEXCOORD1;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);

                float4 clip = OUT.positionHCS;
                OUT.screenPos = float4(
                    clip.x * 0.5 + clip.w * 0.5,
                    clip.y * 0.5 * _ProjectionParams.x + clip.w * 0.5,
                    clip.z,
                    clip.w);

                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;

                #if UNITY_UV_STARTS_AT_TOP
                    screenUV.y = 1.0 - screenUV.y;
                #endif
                screenUV.y = lerp(screenUV.y, 1.0 - screenUV.y, _FlipY);

                half3 view = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, screenUV).rgb * _Brightness;
                half3 tinted = lerp(view, _TintColor.rgb, _TintStrength);

                float dist = length(IN.uv * 2.0 - 1.0);
                half circleMask = 1.0 - smoothstep(1.0 - _EdgeSoftness, 1.0, dist);
                half rim = smoothstep(1.0 - _RimWidth, 1.0, dist)
                         * (1.0 - smoothstep(1.0, 1.0 + _RimWidth, dist));

                half3 color = tinted * circleMask + _RimColor.rgb * rim * _RimPower;
                half  alpha = circleMask * _Alpha + rim;

                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
}
