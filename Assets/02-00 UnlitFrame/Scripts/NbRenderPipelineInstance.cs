using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Profiling;

namespace Catlike0200
{
    public class NbRenderPipelineInstance : RenderPipeline
    {
        CameraRenderer cameraRenderer = new CameraRenderer();
        
       
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach(var camera in cameras)
            {
                cameraRenderer.Render(context, camera);
            }
        }
    }
}
