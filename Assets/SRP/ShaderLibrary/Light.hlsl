#ifndef CUSTOM_LIGHT_INCLUDED
#define CUSTOM_LIGHT_INCLUDED
#define MAX_DIRECTIONAL_LIGHTS_COUNT 4

// 将 CPU 传过来的方向光信息数组存起来
CBUFFER_START(_CustomLight)
    int _DirectionalLightCount;
    float3 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHTS_COUNT];
    float3 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHTS_COUNT];
    float2 _DirectionalLightShadowData[MAX_DIRECTIONAL_LIGHTS_COUNT];
CBUFFER_END

struct Light
{
    float3 color;
    float3 direction;
    float attenuation;  // 阴影影响到的光照衰减，0表示完全在阴影中，1表示完全不在阴影中
};

float GetLightCount()
{
    return _DirectionalLightCount;
    // 这样可以支持 OpenGL ES 2.0 和 WebGL 1.0，因为它能将可变 loop 展开成一系列条件判断局域，但是会变得低效
    //return min(_DirectionalLightCount,MAX_DIRECTIONAL_LIGHTS_COUNT);
}

DirectionalShadowData GetDirectionalShadowData(int index)
{
    DirectionalShadowData data;
    data.strength = _DirectionalLightShadowData[index].x;
    data.tileIndex = _DirectionalLightShadowData[index].y;
    return data;
}

Light GetDirectionLight(int index,Surface surfaceWS)
{
    Light light;
    light.color = _DirectionalLightColors[index];
    light.direction = _DirectionalLightDirections[index];
    light.attenuation = GetDirectionalShadowAttenuation(GetDirectionalShadowData(index),surfaceWS);
    return light;
}

#endif