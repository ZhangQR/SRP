using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Profiling;

namespace Catlike0102
{
    public class NbRenderPipelineInstance : RenderPipeline
    {
        // NbRenderPipelineAsset renderPipelineAsset;
        CameraRenderer cameraRenderer = new CameraRenderer();
        
        //public NbRenderPipelineInstance(NbRenderPipelineAsset asset)
        //{
        //    renderPipelineAsset = asset;
        //}
        
       
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            // 即使在这里采样，Unity 给相机的 48 字节也依然存在
            Profiler.BeginSample("TotalRender");
            foreach(var camera in cameras)
            {
                cameraRenderer.Render(context, camera);
            }
            Profiler.EndSample();
        }
    }
}
