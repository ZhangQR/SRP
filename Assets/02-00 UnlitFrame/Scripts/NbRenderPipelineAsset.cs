using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Catlike0200
{
    [CreateAssetMenu(menuName = "Rendering/Niu Bi Render Pipeline Asset/0200",order =3)]
    public class NbRenderPipelineAsset : RenderPipelineAsset
    {
        protected override RenderPipeline CreatePipeline()
        {
            return new NbRenderPipelineInstance();
        }
    }
}
