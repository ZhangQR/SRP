#ifndef CUSTOM_SURFACE_INCLUDED
#define CUSTOM_SURFACE_INCLUDED

struct Surface
{
    float3 normal;
    float3 color;
    float3 viewDirection;
    float3 position;
    float depth;    // view space depth
    float alpha;
    float metallic;
    float smoothness;
};


#endif