Shader "NiuBiRP/Lit"
{
    /*
    1. 在 surface 中新增两个属性，metallic，smoothness，并且做好 GPU Instance 支持
    2. 新增 BRDF.hlsl，定义 brdf struct，分为：diffuse，specular，roughness，这里的 diffuse，specular主要用于计算入射光分配的比例和基色的融合。	
    3. 在 surface 中新增 viewDirection，在世界坐标中计算，_WorldSpaceCameraPos 不用写在 UnityPerDraw 里面
    4. BRDF.hlsl，GetBRDF (Surface surface)：BRDF
    5. 计算 SpecularStrength (Surface surface, BRDF brdf, Light light):float 在 BRDF.hlsl
    6. DirectBRDF (Surface surface, BRDF brdf, Light light):float3 在 BRDF.hlsl
    7. 在 GetLighting 方法里面新增 brdf 参数，并做好适配
    8. 修改 PerObjectMaterialProperties 
    */
    Properties
    {
        [MainTexture]_BaseMap("Texture",2D) = "white"{}
        _BaseColor("BaseColor",Color) = (0.5,0.5,0.5,1)
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend",float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)]_DesBlend("Des Blend",float) = 0
        [Enum(Off,0,On,1)]_ZWrite("Z Write",float) = 0
        _Clip("Alpha Cutoff",Range(0.0,1.0))=0.5
        [Toggle(_CLIPPING)]_("Clipping",float) = 0
        _Metallic("Metallic",range(0.0,1.0)) = 0
        _Smoothness("Smoothness",range(0.0,1.0)) = 0.5
        // 这个属性名字虽然可以随便取，但是不能有重复
        [Toggle(_TEST)]__("Test",float) = 0
        
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
                #pragma shader_feature _TEST
                #pragma shader_feature _CLIPPING
                #pragma multi_compile_instancing
                #pragma vertex LitPassVertex
			    #pragma fragment LitPassFragment
                #include "LitPass.hlsl"
			    ENDHLSL
            }
    }
}
