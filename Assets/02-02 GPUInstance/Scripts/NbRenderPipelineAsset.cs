using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Catlike0202
{
    [CreateAssetMenu(menuName = "Rendering/Niu Bi Render Pipeline Asset/0202",order =5)]
    public class NbRenderPipelineAsset : RenderPipelineAsset
    {
        // Asset 面板上会出现三个复选框
        [SerializeField] private bool useDynamicBatching = true, useGPUInstancing = true, useSRPBatcher = true;
        protected override RenderPipeline CreatePipeline()
        {
            // 将三个配置传给 Instance，但感觉直接传 this 更省事
            return new NbRenderPipelineInstance(useDynamicBatching,useGPUInstancing,useSRPBatcher);
        }
    }
}
