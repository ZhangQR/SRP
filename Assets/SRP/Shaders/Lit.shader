Shader "NiuBiRP/Lit"
{
    Properties
    {
        [MainTexture]_BaseMap("Texture",2D) = "white"{}
        _BaseColor("BaseColor",Color) = (0.5,0.5,0.5,1)
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend",float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)]_DesBlend("Des Blend",float) = 0
        [Enum(Off,0,On,1)]_ZWrite("Z Write",float) = 0
        _Clip("Alpha Cutoff",Range(0.0,1.0))=0.5
        // 因为我们完全使用自定义编辑器了，所以这一项可以不要
        // [Toggle(_CLIPPING)]_CutOff("Cut Off",float) = 0
        _Metallic("Metallic",range(0.0,1.0)) = 0
        _Smoothness("Smoothness",range(0.0,1.0)) = 0.5
        // 当组合是 One + OneMinusSrcAlpha 的时候，可以打开开关，并且在自己想要受到 Alpha 影响的地方预乘 alpha
        // [Toggle(_PREMULTIPLY_ALPHA)]_PremultiplyAlpha("Premultiply Alpha",float) = 0
        // 这个属性名字虽然可以随便取，但是不能有重复
        // [Toggle(_TEST)]__("Test",float) = 0
        
    }
    SubShader
    {
        Pass
            {
                Tags {"LightMode" = "NiuBiLit"}   
                // 我们想要让 diffuse 受到 alpha 影响，而 specular 保持不变
                // 所以采用 diffuse preMultiplyAlpha 和 One + OneMinusSrcAlpha 的方法        
                Blend [_SrcBlend] [_DesBlend]
                ZWrite [_ZWrite]
                HLSLPROGRAM
                // Equivalent to OpenGL ES 3.0.
                #pragma target 3.5
                #pragma shader_feature _TEST
                #pragma shader_feature _CLIPPING
                #pragma shader_feature _PREMULTIPLY_ALPHA
                #pragma multi_compile_instancing
                #pragma vertex LitPassVertex
			    #pragma fragment LitPassFragment
                #include "LitPass.hlsl"
			    ENDHLSL
            }
        Pass
        {
            Tags {"LightMode" = "ShadowCaster"}
            // 我们只需要用到 depth buffer   
            ColorMask 0
            HLSLPROGRAM
            #pragma target 3.5
            #pragma shader_feature _CLIPPING
            #pragma multi_compile_instancing
            #pragma vertex ShadowCasterPassVertex
			#pragma fragment ShadowCasterPassFragment
            #include "ShadowCasterPass.hlsl"
			ENDHLSL
        }        
        
    }
    //CustomEditor "CustomShaderGUI"
    CustomEditor "CustomShaderGUI2"
}

