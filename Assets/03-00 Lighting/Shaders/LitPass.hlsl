#ifndef CUSTOM_Lit_PASS_INCLUDED
#define CUSTOM_LIT_PASS_INCLUDED

#include "../ShaderLibrary/Common.hlsl"

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
    UNITY_DEFINE_INSTANCED_PROP(float4,_BaseMap_ST)
    #if _CLIPPING
    UNITY_DEFINE_INSTANCED_PROP(float,_Clip)
    #endif
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

struct Attribution
{
    float3 positionOS:POSITION;
    // normal 是 3 位，tangent 是 4，并且 w 是 -1/1，用来区分 bitangent 方向
    float3 normalOS :NORMAL;
    float2 uv       :TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionHCS:SV_POSITION;
    float3 normalWS :VAR_NORMAL_WS;
    float2 uv       :VAR_UV;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings LitPassVertex (Attribution input) {
    Varyings output;
    
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input,output);

    float3 positionWS = TransformObjectToWorld(input.positionOS);
    output.positionHCS = TransformWorldToHClip(positionWS);

    // 内部有 normalize，尽管后面还要再进行一次，这次也不能省
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);

    float4 basemap_ST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseMap_ST);
    output.uv = input.uv * basemap_ST.xy + basemap_ST.zw;
    return output;
}


half4 LitPassFragment (Varyings input):SV_TARGET
{
    // 可视化 normalWS
    // return half4(input.normalWS.xyz,1.0);

    // 在 vertex shader 的计算时逐顶点的，然后经过 interpolation，会使三角形内部的 normal 不再是单位长度
    // 所以要再进行一次 normalized，第一次 normalize 是在 vertex shader
    return half4((abs(length(input.normalWS)-1) * 20).xxx,1.0);
    float3 normalWS = SafeNormalize(input.normalWS);
    return half4(normalWS.xyz,1.0);

    UNITY_SETUP_INSTANCE_ID(input);
    half4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseColor);
    half4 textureColor = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.uv);
    half4 finalColor = baseColor * textureColor;
    #if _CLIPPING
    clip(finalColor.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Clip));
    #endif
    return finalColor;
    
}
#endif
