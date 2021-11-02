using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShadowSetting
{
    // 基于 view space depth
    // 因为要作为分母，所以限制最小值不为 0
    [Min(0.001f)]
    public float maxDistance = 100;
    
    // 因为要作为分母，所以限制最小值不为 0
    [Range(0.001f, 1.0f)] public float distanceFade = 1;

    public enum TextureSize
    {
        _128 = 128,
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096
    };
    
    [System.Serializable]
    public struct Directional
    {
        public TextureSize altasSize;
        [Range(1, 4)] public int CascadeCount; 
        [Range(0.0f, 1.0f)] public float CascadeRatio1, CascadeRatio2, CascadeRatio3;
        [Range(0.001f, 1.0f)] public float CascadeFade;
        
        // ComputeDirectionalShadowMatricesAndCullingPrimitives 需要用到 Vector3 形式的
        public Vector3 CascadeRatios =>
            new Vector3(CascadeRatio1, CascadeRatio2, CascadeRatio3);
    }

    // 设置默认值
    public Directional directional = new Directional
    {
        altasSize = TextureSize._256,
        CascadeCount = 1,
        CascadeRatio1 = 0.1f,
        CascadeRatio2 = 0.25f,
        CascadeRatio3 = 0.5f,
        CascadeFade = 0.1f
    };
}
