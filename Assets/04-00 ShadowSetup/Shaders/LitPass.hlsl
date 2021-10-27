#ifndef CUSTOM_Lit_PASS_INCLUDED
#define CUSTOM_LIT_PASS_INCLUDED

#include "../ShaderLibrary/Common.hlsl"
#include "../ShaderLibrary/Surface.hlsl"
#include "../ShaderLibrary/Light.hlsl"
#include "../ShaderLibrary/BRDF.hlsl"
#include "../ShaderLibrary/Lighting.hlsl"

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
    UNITY_DEFINE_INSTANCED_PROP(float4,_BaseMap_ST)
    UNITY_DEFINE_INSTANCED_PROP(float,_Metallic)
    UNITY_DEFINE_INSTANCED_PROP(float,_Smoothness)
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
    float3 positionWS:VAR_POSITION_WS;
    float3 normalWS :VAR_NORMAL_WS;
    float2 uv       :VAR_UV;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings LitPassVertex (Attribution input) {
    Varyings output;
    
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input,output);

    // 方便后面在世界坐标中计算 viewDirection
    output.positionWS = TransformObjectToWorld(input.positionOS);
    output.positionHCS = TransformWorldToHClip(output.positionWS);
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);

    float4 basemap_ST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseMap_ST);
    output.uv = input.uv * basemap_ST.xy + basemap_ST.zw;
    return output;
}


half4 LitPassFragment (Varyings input):SV_TARGET
{   
    UNITY_SETUP_INSTANCE_ID(input);
    half4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseColor);
    half4 textureColor = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.uv);
    half4 finalColor = baseColor * textureColor;

    Surface surface;
    surface.normal = normalize(input.normalWS);
    surface.color = finalColor.xyz;
    surface.alpha = finalColor.a;
    surface.metallic = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Metallic);
    surface.smoothness = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Smoothness);
    surface.viewDirection = normalize(_WorldSpaceCameraPos-input.positionWS);

    BRDF brdf;
    #if defined(_PREMULTIPLY_ALPHA)
    brdf = GetBRDF(surface,true);
    #else
    brdf = GetBRDF(surface,false);
    #endif

    #if _CLIPPING
    clip(finalColor.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Clip));
    #endif
    finalColor.xyz = GetLighting(surface,brdf);

    return half4(finalColor);


    
}
#endif
