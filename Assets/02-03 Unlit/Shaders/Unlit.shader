Shader "NiuBiRP/Unlit/0203"
{
    // 1、设置透明，设置 queue，blend，Zwrite
    // 2、主贴图,贴图 property，ST，UV
    // 3、Clip，Toggle，clip value,shader_feature
    Properties
    {
        
        [MainTexture]_BaseMap("Texture",2D) = "white"{}
        _BaseColor("BaseColor",Color) = (1,1,1,1)
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend",float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)]_DesBlend("Des Blend",float) = 0
        [Enum(Off,0,On,1)]_ZWrite("Z Write",float) = 0
        _Clip("Alpha Cutoff",Range(0.0,1.0))=0.5    // cutoff
        [Toggle(_CLIPPING)]_("Clipping",float) = 0
        
    }
    SubShader
    {
        Pass
            {
                // 设置透明混合模式			
                Blend [_SrcBlend] [_DesBlend]
                // 关闭深度写入
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
}
