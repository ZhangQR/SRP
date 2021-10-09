// 1、使用 SRP batch 设置基色
// 2、尝试使用 MaterialPropertyBlock 来控制 Material 属性（SRP Batching 会失效）
Shader "NiuBiRP/Unlit/0201"
{
    Properties
    {
        // 定义基色
        _BaseColor("BaseColor",Color) = (1,1,1,1)
    }
    SubShader
    {
        Pass
        {			
            HLSLPROGRAM
            #pragma vertex UnlitPassVertex
			#pragma fragment UnlitPassFragment
            #include "UnlitPass.hlsl"
			ENDHLSL
        }
    }
}
