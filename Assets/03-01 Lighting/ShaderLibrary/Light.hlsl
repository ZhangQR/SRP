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
}

Light GetDirectionLight(int index)
{
    Light light;
    light.color = _DirectionalLightColors[index];
    light.direction = _DirectionalLightDirections[index];
    return light;
}

#endif