using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShadowSetting
{
    [Min(0f)]
    public float maxDistance = 100;
    
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
    }

    // 设置默认值
    public Directional directional = new Directional
    {
        altasSize = TextureSize._256
    };
}
