#ifndef CUSTOM_SHADOW_CASTER_PASS_INCLUDED
#define CUSTOM_SHADOW_CASTER_PASS_INCLUDED

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

Varyings ShadowCasterPassVertex (Attribution input) {
    Varyings output;
    
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input,output);
    output.positionWS = TransformObjectToWorld(input.positionOS);
    output.positionHCS = TransformWorldToHClip(output.positionWS);
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);

    float4 basemap_ST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseMap_ST);
    output.uv = input.uv * basemap_ST.xy + basemap_ST.zw;
    return output;
}


// 不需要返回值，所以使用 void 即可
void ShadowCasterPassFragment (Varyings input)
{   
    UNITY_SETUP_INSTANCE_ID(input);
    half4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseColor);
    half4 textureColor = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.uv);
    half4 finalColor = baseColor * textureColor;


    #if _CLIPPING
    clip(finalColor.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Clip));
    #endif    
}
#endif
