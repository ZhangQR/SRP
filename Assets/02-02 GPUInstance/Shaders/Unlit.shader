Shader "NiuBiRP/Unlit/0202"
{
    Properties
    {
        _BaseColor("BaseColor",Color) = (1,1,1,1)
    }
    SubShader
    {
        Pass
        {			
            HLSLPROGRAM
            // 生成两种 shader variants，一种不带 GPU Instance，一种带
            // 在 Material Inspector 面板上有复选框
            #pragma multi_compile_instancing
            #pragma vertex UnlitPassVertex
			#pragma fragment UnlitPassFragment
            #include "UnlitPass.hlsl"
			ENDHLSL
        }
    }
}
