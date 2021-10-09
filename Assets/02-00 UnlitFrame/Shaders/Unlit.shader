Shader "NiuBiRP/Unlit/0200"
{
    Properties
    {
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
