// 防止重复包含的头文件预处理
#ifndef CUSTOM_UNLIT_PASS_INCLUDED
#define CUSTOM_UNLIT_PASS_INCLUDED

#include "../ShaderLibrary/Common.hlsl"


float4 UnlitPassVertex (float3 positionOS : POSITION) : SV_POSITION {
    float3 positionWS = TransformObjectToWorld(positionOS);
    return TransformWorldToHClip(positionWS);
}

// 移动端尽可能区分 half 和 float，通常只有位置和 uv 坐标使用 float，其他使用 half
// 非移动端不管写什么都会变成 float，因为 GPU 总是使用 float。
// fixed 在老设备上，基本相当于 half
half4 UnlitPassFragment ():SV_TARGET
{
    return 0.0;
}
#endif