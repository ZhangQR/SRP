using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace NiuBiRenderPipeline
{
    /// <summary>
    /// 中文名：牛逼渲染管线
    /// 学习：https://catlikecoding.com/unity/tutorials/custom-srp/custom-render-pipeline/
    /// </summary>
    public class NbRenderPipelineInstance : RenderPipeline
    {
        NbRenderPipelineAsset renderPipelineAsset;
        
        // 新建一个相机渲染器，使用这种渲染方式的相机可以共享这一个渲染器
        CameraRenderer cameraRenderer = new CameraRenderer();
        
        /// <summary>
        /// instance 的构造函数，当管线的设置或者代码有什么变化时，Unity 会销毁原来的 Instance，并且新建一个
        /// </summary>
        /// <param name="asset">管线的配置，实际上是一个 Scriptable Object</param>
        public NbRenderPipelineInstance(NbRenderPipelineAsset asset)
        {
            renderPipelineAsset = asset;
        }
        
        
        /// <summary>
        /// 渲染主循环/入口
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cameras"></param>
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach(var camera in cameras)
            {
                cameraRenderer.Render(context, camera);
            }

            // 保留一个简单正常，按顺序的逻辑
            //var cmd = new CommandBuffer();
            //cmd.name = "ExampleCommandBuffer";
            //cmd.ClearRenderTarget(true, true, Color.white);
            //context.ExecuteCommandBuffer(cmd);
            //cmd.Release();
            //context.Submit();
        }
    }
}
