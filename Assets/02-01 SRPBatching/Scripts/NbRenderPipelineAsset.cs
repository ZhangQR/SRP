using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Catlike0201
{
    [CreateAssetMenu(menuName = "Rendering/Niu Bi Render Pipeline Asset/0201",order =4)]
    public class NbRenderPipelineAsset : RenderPipelineAsset
    {
        protected override RenderPipeline CreatePipeline()
        {
            return new NbRenderPipelineInstance();
        }
    }
}
