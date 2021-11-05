#ifndef CUSTOM_SHADOWS_INCLUDED
#define CUSTOM_SHADOWS_INCLUDED
#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT  4
#define MAX_CASCADE_COUNT  4

TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_CustomShadows)
    float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT * MAX_CASCADE_COUNT];
    int _CascadeCount;
    float4 _CascadeCullSpheres[MAX_CASCADE_COUNT];
    float4 _ShaderDistanceFade; // x: 1/maxShadowDistance, y:1/distanceFade ,z:1/(1-(1-cascadeFade)^2)
    float4 _CascadeData[MAX_CASCADE_COUNT];//x: 1/cullSphere.w(半径)
CBUFFER_END

// 1 完全在阴影中
struct DirectionalShadowData
{
    float strength;
    int tileIndex;
    float normalBias;
};

struct ShadowData
{
    int cascadeIndex;
    // 0 表示没有阴影
    float strength;
};

// 在边缘渐变，公式： (1-d/m)/fade
// d: depth
// m: maxDistance
// fade: 0-1, 1 绘制阴影，0 不绘制阴影
float FadedShadowStrength (float distance,float oneUpFade,float oneUpMax)
{
    return saturate((1-distance*oneUpMax)*oneUpFade);
}

ShadowData GetShadowData(Surface surfaceWS)
{
    ShadowData shadowData;
    // 当超出最大距离时，也没有阴影，是 culling plane
    shadowData.strength = FadedShadowStrength(
        surfaceWS.depth,_ShaderDistanceFade.y,_ShaderDistanceFade.x);
    int i = 0;
    for(; i<_CascadeCount; i++)
    {
        float4 sphere = _CascadeCullSpheres[i];
        float distanceSqr = DistanceSquared(surfaceWS.position, sphere.xyz); 
        if(distanceSqr < sphere.w)
        {
            // 最后一个 cascade 进行边缘渐变
            if(i == _CascadeCount-1)
            {
                shadowData.strength *= FadedShadowStrength(
                    distanceSqr,_ShaderDistanceFade.z,_CascadeData[i].x);
            }
            break;
        }
    }
    // 当超出最大 cascade 的时候，没有阴影，是 culling sphere
    if(i == _CascadeCount)
    {
        shadowData.strength = 0;
    }
    shadowData.cascadeIndex = i;
    return shadowData;
}

// sts = shadow texture space
// return: 0-1 0完全在阴影中，1完全不在阴影中
float SampleDirectionalShadowAtlas(float3 positionSTS)
{
    return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowAtlas,SHADOW_SAMPLER,positionSTS);
}

float GetDirectionalShadowAttenuation(DirectionalShadowData  directional,
    ShadowData global,Surface surfaceWS)
{
    // 当灯光完全不需要绘制阴影的时候，data 是 0,0，可以直接跳过，并且不会影响效率，因为分支的选择都一样
    // 这里不加这个判断结果是一样，但是要多采样一次，影响效率
    if(directional.strength<=0)
    {
        return 1.0f;
    }
    float3 normalBias = surfaceWS.normal *
        (directional.normalBias *_CascadeData[global.cascadeIndex].y);
    float3 positionSTS = mul(_DirectionalShadowMatrices[directional.tileIndex],
        float4(surfaceWS.position + normalBias,1)).xyz;
    float strength = directional.strength;
    // strength 跟 attenuation 是相反的
    return lerp(1,SampleDirectionalShadowAtlas(positionSTS),strength);
}


#endif