using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Catlike0101
{
    public class NbRenderPipelineInstance : RenderPipeline
    {
        NbRenderPipelineAsset renderPipelineAsset;
        CameraRenderer cameraRenderer = new CameraRenderer();
        
        public NbRenderPipelineInstance(NbRenderPipelineAsset asset)
        {
            renderPipelineAsset = asset;
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
