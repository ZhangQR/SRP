Shader "NiuBiRP/Lit"
{
    // 1、 设置 lightmode 并且与 draw visible object 相匹配
    // 2、 设置 normal 并且可视化经过 interpolation normal 的变形
    Properties
    {
        [MainTexture]_BaseMap("Texture",2D) = "white"{}
        _BaseColor("BaseColor",Color) = (0.5,0.5,0.5,1)
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend",float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)]_DesBlend("Des Blend",float) = 0
        [Enum(Off,0,On,1)]_ZWrite("Z Write",float) = 0
        _Clip("Alpha Cutoff",Range(0.0,1.0))=0.5
        [Toggle(_CLIPPING)]_("Clipping",float) = 0
        
    }
    SubShader
    {
        Pass
            {
                Tags {"LightMode" = "NiuBiLit"}           
                Blend [_SrcBlend] [_DesBlend]
                ZWrite [_ZWrite]
                HLSLPROGRAM
                // Equivalent to OpenGL ES 3.0.
                #pragma target 3.5
                #pragma shader_feature _CLIPPING
                #pragma multi_compile_instancing
                #pragma vertex LitPassVertex
			    #pragma fragment LitPassFragment
                #include "LitPass.hlsl"
			    ENDHLSL
            }
    }
}
