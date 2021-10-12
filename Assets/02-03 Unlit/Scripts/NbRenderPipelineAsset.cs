using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Catlike0203
{
    [CreateAssetMenu(menuName = "Rendering/Niu Bi Render Pipeline Asset/0203",order =6)]
    public class NbRenderPipelineAsset : RenderPipelineAsset
    {
        [SerializeField] private bool useDynamicBatching = true, useGPUInstancing = true, useSRPBatcher = true;
        protected override RenderPipeline CreatePipeline()
        {
            return new NbRenderPipelineInstance(useDynamicBatching,useGPUInstancing,useSRPBatcher);
        }
    }
}
