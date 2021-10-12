#ifndef CUSTOM_UNLIT_PASS_INCLUDED
#define CUSTOM_UNLIT_PASS_INCLUDED

#include "../ShaderLibrary/Common.hlsl"

// 定义 _BaseMap 和 sampler
TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
    // 定义 _BaseMap 的 Offset 和 Scale
    UNITY_DEFINE_INSTANCED_PROP(float4,_BaseMap_ST)
    #if _CLIPPING
    UNITY_DEFINE_INSTANCED_PROP(float,_Clip)
    #endif
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

struct Attribution
{
    float3 positionOS:POSITION;
    // 有了贴图，就要有 uv
    float2 uv       :TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionHCS:SV_POSITION;
    float2 uv       :VAR_UV;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings UnlitPassVertex (Attribution input) {
    Varyings output;
    
    UNITY_SETUP_INSTANCE_ID(input);
  
    UNITY_TRANSFER_INSTANCE_ID(input,output);

    float3 positionWS = TransformObjectToWorld(input.positionOS);
    output.positionHCS = TransformWorldToHClip(positionWS);

    // uv 应用 ST
    float4 basemap_ST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseMap_ST);
    output.uv = input.uv * basemap_ST.xy + basemap_ST.zw;
    return output;
}


half4 UnlitPassFragment (Varyings input):SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    half4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseColor);
    // 采样 _BaseMap
    half4 textureColor = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.uv);
    // 混合
    half4 finalColor = baseColor * textureColor;
    #if _CLIPPING
    // #if defined(_CLIPPING)
    clip(finalColor.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Clip));
    #endif
    return finalColor;
}
#endif
