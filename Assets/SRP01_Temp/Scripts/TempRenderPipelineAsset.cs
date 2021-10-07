using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "CustomRenderPipeline/TempRenderPipelineAsset")]
public class TempRenderPipelineAsset:RenderPipelineAsset
{
    
    protected override RenderPipeline CreatePipeline()
        {
            return new TempRenderPipelineInstance(this);
        }
}
