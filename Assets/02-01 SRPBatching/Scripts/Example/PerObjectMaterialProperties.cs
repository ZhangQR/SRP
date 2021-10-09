using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
// Random.value 是 UnityEngine 里面的
using Random = UnityEngine.Random;

/// <summary>
/// 可以让每个物体有不同的 material property
/// </summary>
public class PerObjectMaterialProperties : MonoBehaviour
{
    [SerializeField] Color baseColor;
    static int baseColorId = Shader.PropertyToID("_BaseColor");
    
    // 可以复用 
    static MaterialPropertyBlock block;

    // 运行之后更新一次 material 中属性的值
    private void Awake()
    {
        baseColor = new Color(Random.value, Random.value, Random.value);
        OnValidate();
    }

    // 当 Inspector 上的值改变的时候调用，重新设置 material 中属性的值
    void OnValidate()
    {
        if (block == null)
        {
            block = new MaterialPropertyBlock();
        }
        block.SetColor(baseColorId,baseColor);
        
        // 使用了 MaterialPropertyBlock 之后，SRP Batching 将会失效
        // 除这个脚本之后，material 不会变回来，需要在移除之前执行一次 null
        // GetComponent<Renderer>().SetPropertyBlock(null);
        GetComponent<Renderer>().SetPropertyBlock(block);
    }
}
