#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

// 计算光源真实的辐照度（Irradiance），每单位面积功率
float3 IncomingLight (Surface surface, Light light) {
    // 半波兰特
    // return saturate(0.5 * dot(surface.normal, light.direction) + 0.5) * light.color;
    return saturate(dot(surface.normal, light.direction) * light.attenuation) * light.color;
}

// 指定光源的光照
float3 GetLighting(Surface surfaceWS,BRDF brdf,Light light)
{
    return surfaceWS.color * IncomingLight(surfaceWS,light) * DirectBRDF(surfaceWS,brdf,light);
}

// 将所有的灯光的影响加起来
float3 GetLighting(Surface surfaceWS,BRDF brdf)
{
    float3 color = 0.0;
    ShadowData shadowData = GetShadowData(surfaceWS);
    for (int i = 0;i<GetLightCount();i++)
    {
        Light light = GetDirectionLight(i,surfaceWS,shadowData);
        color += GetLighting(surfaceWS,brdf,light);
    }
    return color;
}
#endif