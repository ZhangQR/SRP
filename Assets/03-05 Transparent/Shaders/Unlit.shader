Shader "NiuBiRP/Unlit"
{
    Properties
    {
        
        [MainTexture]_BaseMap("Texture",2D) = "white"{}
        _BaseColor("BaseColor",Color) = (1,1,1,1)
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend",float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)]_DesBlend("Des Blend",float) = 0
        [Enum(Off,0,On,1)]_ZWrite("Z Write",float) = 0
        _Clip("Alpha Cutoff",Range(0.0,1.0))=0.5
        // 这个属性名要跟 Lit 的一样，因为他们是用的同一个 ShaderGUI
        [Toggle(_CLIPPING)]_CutOff("Clipping",float) = 0
        
    }
    SubShader
    {
        Pass
            {
                Blend [_SrcBlend] [_DesBlend]
                ZWrite [_ZWrite]
                HLSLPROGRAM
                #pragma shader_feature _CLIPPING
                #pragma multi_compile_instancing
                #pragma vertex UnlitPassVertex
			    #pragma fragment UnlitPassFragment
                #include "UnlitPass.hlsl"
			    ENDHLSL
            }
    }
    CustomEditor "CustomShaderGUI"
}
