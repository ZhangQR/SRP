using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "CustomRenderPipeline/ExampleRenderPipelineAsset")]
public class ExampleRenderPipelineAsset:RenderPipelineAsset
{
    // 这些属性会被显示在 Inspector 上
    public Color exampleColor;
    public string exampleString;
    
    // Unity 在绘制每一帧之前调用它，如果配置有变换，就摧毁当前的 Pipeline，并且重新创建一个
    protected override RenderPipeline CreatePipeline()
    {
        // 实例化一个 Render Pipeline 给 SRP 渲染，并且传递 Asset 的引用
        // Render Pipeline 可以读取 Asset 的配置
        return new ExampleRenderPipelineInstance(this);
    }
}
