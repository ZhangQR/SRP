#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

// 计算光源真实的辐照度（Irradiance），每单位面积功率
float3 IncomingLight (Surface surface, Light light) {
    // 半波兰特
    // return saturate(0.5 * dot(surface.normal, light.direction) + 0.5) * light.color;
    return saturate(dot(surface.normal, light.direction)) * light.color;
}

// 指定光源的光照
float3 GetLighting(Surface surface,Light light)
{
    return surface.color * IncomingLight(surface,light);
}

// 将所有的灯光的影响加起来
float3 GetLighting(Surface surface)
{
    float3 color = 0.0;
    for (int i = 0;i<GetLightCount();i++)
    {
        color += GetLighting(surface,GetDirectionLight(i));
    }
    return color;
}
#endif