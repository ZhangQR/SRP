#ifndef CUSTOM_LIGHT_INCLUDED
#define CUSTOM_LIGHT_INCLUDED
#define MAX_DIRECTIONAL_LIGHTS_COUNT 4

// 将 CPU 传过来的方向光信息数组存起来
CBUFFER_START(_CustomLight)
    int _DirectionalLightCount;
float4 _DirectionalLightColors[4];
float4 _DirectionalLightDirections[4];
CBUFFER_END

struct Light
{
    float3 color;
    float3 direction;
};


float GetLightCount()
{
    return _DirectionalLightCount;
    // 这样可以支持 OpenGL ES 2.0 和 WebGL 1.0，因为它能将可变 loop 展开成一系列条件判断局域，但是会变得低效
    //return min(_DirectionalLightCount,MAX_DIRECTIONAL_LIGHTS_COUNT);
}

Light GetDirectionLight(int index)
{
    Light light;
    light.color = _DirectionalLightColors[index];
    light.direction = _DirectionalLightDirections[index];
    return light;
}

#endif