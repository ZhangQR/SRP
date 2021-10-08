using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Catlike0101
{
    [CreateAssetMenu(menuName = "Rendering/Niu Bi Render Pipeline Asset/0101",order =1)]
    public class NbRenderPipelineAsset : RenderPipelineAsset
    {
        protected override RenderPipeline CreatePipeline()
        {
            return new NbRenderPipelineInstance(this);
        }
    }
}
