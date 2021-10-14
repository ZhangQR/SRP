#ifndef CUSTOM_SURFACE_INCLUDED
#define CUSTOM_SURFACE_INCLUDED

// lighting 就是模拟光线和物体表面的作用方式，所以我们可以简单的拆成：
// Object Surface Information
// Light Information
struct Surface
{
    // 我们有可能在任何空间进行光照计算，所以不写成 normalWS，或者 normalTS
    // 只需要在后面计算的时候保持空间一致即可
    float3 normal;

    // 因为 alpha 存放的不一定是透明信息，所以单独存放
    float3 color;
    float alpha;
};


#endif