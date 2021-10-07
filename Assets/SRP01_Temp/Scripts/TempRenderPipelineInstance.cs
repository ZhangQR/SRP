using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TempRenderPipelineInstance : RenderPipeline
{
    TempRenderPipelineAsset renderPipelineAsset;
    CameraRenderer cameraRenderer = new CameraRenderer();

    public TempRenderPipelineInstance(TempRenderPipelineAsset asset)
    {
        renderPipelineAsset = asset;
    }
    
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        // 不要在这个类中渲染所有的相机，而是把每一个相机都分配给 cameraRenderer 去渲染
        // 后面可拓展每种相机以不同的方式进行渲染，但是现在都是同一种方式
        foreach (var camera in cameras)
        {
            cameraRenderer.Render(context, camera);
        }
    }
}
