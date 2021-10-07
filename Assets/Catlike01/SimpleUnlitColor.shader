// 定义了一个简单的 Unlit Shader，来兼容我们的自定义 RP
// 有一个写死的颜色，描述了 LightMode Tag 的使用
// 这个着色器不兼容 SRP Batcher
// 官方示例：https://docs.unity3d.com/Manual/srp-creating-simple-render-loop.html

Shader "Examples/SimpleUnlitColor"
{
    SubShader
    {
        Pass
        {
            // LightMode 的值一定要和 ScriptableRenderContext.DrawRenderers 中的 ShaderTagId 相匹配 
            Tags { "LightMode" = "ExampleLightModeTag"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4x4 unity_MatrixVP;
            float4x4 unity_ObjectToWorld;

            struct Attributes
            {
                float4 positionOS   : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                float4 worldPos = mul(unity_ObjectToWorld, IN.positionOS);
                OUT.positionCS = mul(unity_MatrixVP, worldPos);
                return OUT;
            }

            float4 frag (Varyings IN) : SV_TARGET
            {
                return float4(0.5,1,0.5,1);
            }
            ENDHLSL
        }
    }
}