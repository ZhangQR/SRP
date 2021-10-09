#ifndef CUSTOM_UNLIT_PASS_INCLUDED
#define CUSTOM_UNLIT_PASS_INCLUDED

#include "../ShaderLibrary/Common.hlsl"

// 属性定义在全局
// float4 _BaseColor;

// 定义在 UnityPerMaterial Constant buffers，但不是所有平台都支持 Constant buffers
// 比如 OpenGL ES 2.0 就不支持
// cbuffer UnityPerMaterial {
//     float _BaseColor;
// };

// 一般采用这种方式
// 要启动 SRP Batches 的必备条件之一，缓存材质的部分属性到 GPU，可以减少 setpass
// 另一个条件是要把一些矩阵放到 UnityPerDraw 中
CBUFFER_START(UnityPerMaterial)
    float4 _BaseColor;
CBUFFER_END

float4 UnlitPassVertex (float3 positionOS : POSITION) : SV_POSITION {
    float3 positionWS = TransformObjectToWorld(positionOS);
    return TransformWorldToHClip(positionWS);
}


half4 UnlitPassFragment ():SV_TARGET
{
    return _BaseColor;
}
#endif