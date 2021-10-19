#ifndef CUSTOM_BRDF_INCLUDED
#define CUSTOM_BRDF_INCLUDED

struct BRDF
{
    real3 diffuse;
    real3 specular;
    float roughness;
};


#define MIN_REFLECTIVITY 0.04

float OneMinusReflectivity(float metallic)
{
    return 0.96 * (1-metallic);
}

// float OneMinusReflectivity (float metallic) {
//     float range = 1.0 - MIN_REFLECTIVITY;
//     return range - metallic * range;
// }


BRDF GetBRDF (Surface surface,bool isMultiplyAlpha)
{
    BRDF brdf;
    float oneMinusReflectivity = OneMinusReflectivity(surface.metallic);
    brdf.diffuse = surface.color * oneMinusReflectivity;
    if(isMultiplyAlpha)
    {
        brdf.diffuse *= surface.alpha;
    }
    brdf.specular = lerp(MIN_REFLECTIVITY, surface.color,surface.metallic);
    float perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(surface.smoothness);
    brdf.roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
    return brdf;
}

// 因为用的很多，所以自己写一个免得使用耗时的 Pow
float Square (float v) {
    return v * v;
}

// Unity 优化过的 Cook-Torrance
// https://community.arm.com/c/e/40
float SpecularStrength (Surface surface, BRDF brdf, Light light)
{
    float3 h = SafeNormalize(light.direction + surface.viewDirection);
    float nh2 = Square(saturate(dot(surface.normal, h)));
    float lh2 = Square(saturate(dot(light.direction, h)));
    float r2 = Square(brdf.roughness);
    float d2 = Square(nh2 * (r2 - 1.0) + 1.00001);
    float normalization = brdf.roughness * 4.0 + 2.0;
    return r2 / (d2 * max(0.1, lh2) * normalization);
}

float3 DirectBRDF (Surface surface, BRDF brdf, Light light)
{
    return SpecularStrength(surface,brdf,light) * brdf.specular + brdf.diffuse;
}


#endif