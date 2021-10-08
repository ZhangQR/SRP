using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Catlike0100
{
    /// <summary>
    /// 中文名：牛逼渲染管线配置
    /// https://catlikecoding.com/unity/tutorials/custom-srp/custom-render-pipeline/
    /// </summary>
    [CreateAssetMenu(menuName = "Rendering/Niu Bi Render Pipeline Asset/0100",order = 0)]
    public class NbRenderPipelineAsset : RenderPipelineAsset
    {
        protected override RenderPipeline CreatePipeline()
        {
            return new NbRenderPipelineInstance(this);
        }
    }
}
