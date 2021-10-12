using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Profiling;

namespace Catlike0202
{
    public class NbRenderPipelineInstance : RenderPipeline
    {
        CameraRenderer cameraRenderer = new CameraRenderer();
        private bool useDynamicBatching;
        private bool useGPUInstance;

        public NbRenderPipelineInstance(bool useDynamicBatching,bool useGPUInstance,bool useSRPBatcher)
        {
            GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
            this.useDynamicBatching = useDynamicBatching;
            this.useGPUInstance = useGPUInstance;
        }
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach(var camera in cameras)
            {
                cameraRenderer.Render(context, camera,useDynamicBatching,useGPUInstance);
            }
        }
    }
}
