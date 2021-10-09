using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Profiling;

namespace Catlike0201
{
    /// <summary>
    /// 1、新增 SRP 开关
    /// </summary>
    public class NbRenderPipelineInstance : RenderPipeline
    {
        CameraRenderer cameraRenderer = new CameraRenderer();

        public NbRenderPipelineInstance()
        {
            // 开启 SRP batching，其实不写这个默认也会开启，但我们还需要自由控制开启与否
            GraphicsSettings.useScriptableRenderPipelineBatching = true;
        }
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach(var camera in cameras)
            {
                cameraRenderer.Render(context, camera);
            }
        }
    }
}
