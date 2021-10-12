#ifndef CUSTOM_UNLIT_PASS_INCLUDED
#define CUSTOM_UNLIT_PASS_INCLUDED

#include "../ShaderLibrary/Common.hlsl"

// CBUFFER_START(UnityPerMaterial)
//     float4 _BaseColor;
// CBUFFER_END

// 不需要使用 CBUFFER_START，但我们依然把属性放到了 UnityPerMaterial 中，所以还是可以用 SRP Batching 的
UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    // float4 _BaseColor;
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

// GPU 按照 instanced data array 的顺序来绘制，所以需要知道 index
// index 在 vertex data 中，在 UnityInstancing.hlsl 有宏可以很方便的获取 index
// 但是它默认我们的 vertex shader input 是 struct，所以我们要写一个 struct
struct Attribution
{
    float3 positionOS:POSITION;
    // 这个写在 vertex shader input/output struct 里面
    // 使用 UNITY_GET_INSTANCE_ID 获取 instance id
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionHCS:SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings UnlitPassVertex (Attribution input) {
    Varyings output;
    
    // 应该写在 vertex shader / fragment shader 的最开始，他可以从 input 中获得 unity_InstanceID
    // 并把它存在一个 global static variable 中，使得后面的宏可以方便的获得 unity_InstanceID
    UNITY_SETUP_INSTANCE_ID(input);

    // 将 Vertex shader input struct 中的 id copy 到 output struct 中去  
    UNITY_TRANSFER_INSTANCE_ID(input,output);

    float3 positionWS = TransformObjectToWorld(input.positionOS);
    output.positionHCS = TransformWorldToHClip(positionWS);
    return output;
}


half4 UnlitPassFragment (Varyings input):SV_TARGET
{
    // 同 vertex shader 一开始
    UNITY_SETUP_INSTANCE_ID(input);

    // 上面已经设置过了 instance id，这里直接从 instanced data array 中获取想要的属性
    half4 finalColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseColor);
    return finalColor;
}
#endif