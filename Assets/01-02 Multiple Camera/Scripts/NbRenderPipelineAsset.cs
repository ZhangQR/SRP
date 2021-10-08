using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Catlike0102
{
    [CreateAssetMenu(menuName = "Rendering/Niu Bi Render Pipeline Asset/0102",order =2)]
    public class NbRenderPipelineAsset : RenderPipelineAsset
    {
        protected override RenderPipeline CreatePipeline()
        {
            return new NbRenderPipelineInstance(this);
        }
    }
}
