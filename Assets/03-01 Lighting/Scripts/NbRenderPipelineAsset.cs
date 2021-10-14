using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace NiuBiSRP
{
    [CreateAssetMenu(menuName = "Rendering/NiuBi Render Pipeline Asset")]
    public class NbRenderPipelineAsset : RenderPipelineAsset
    {
        [SerializeField] private bool useDynamicBatching = true, useGPUInstancing = true, useSRPBatcher = true;
        protected override RenderPipeline CreatePipeline()
        {
            return new NbRenderPipelineInstance(useDynamicBatching,useGPUInstancing,useSRPBatcher);
        }
    }
}
