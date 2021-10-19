using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Profiling;

namespace NiuBiSRP
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
            // 因为直接使用了 VisibleLight.finalColor，所以这里要设置为线性
            GraphicsSettings.lightsUseLinearIntensity = true;
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