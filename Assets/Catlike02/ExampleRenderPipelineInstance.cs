using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Catlike02
{
    /// <summary>
    /// 官方示例：https://catlikecoding.com/unity/tutorials/custom-srp/custom-render-pipeline/
    /// </summary>
    public class ExampleRenderPipelineInstance : RenderPipeline
    {
        // 保存 Asset 的引用
        private ExampleRenderPipelineAsset renderPipelineAsset;

        // 构造函数接受了一个 Asset
        public ExampleRenderPipelineInstance(ExampleRenderPipelineAsset asset)
        {
            renderPipelineAsset = asset;
        }

        // 渲染的主入口，Unity 会自动调用它，你也可以使用 RenderPipelineManager
        // 中的 4 个回调来控制特定时间发生什么事情
        // 渲染主循环主要有三步： Clear Cull Render,ref: https://docs.unity3d.com/Manual/srp-creating-simple-render-loop.html
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            // 展示如何使用 Asset 中的配置
            // Debug.Log(renderPipelineAsset.exampleString);

            // ********************************************************************************
            // ********************************************************************************
            // ****    Clear  :Clean Last Frame                                            ****
            // ****                                                                        ****
            // ********************************************************************************
            // ********************************************************************************

            // 你可以在这里写自定义的渲染方法
            // 有两种方式可以执行渲染命令
            // 一、提交 CommandBuffers
            var cmd = new CommandBuffer();
            cmd.name = "ExampleCommandBuffer";
            cmd.ClearRenderTarget(true, true, renderPipelineAsset.exampleColor);
            // 把命令添加进 context，在没有 submit 之前是不会被执行的
            context.ExecuteCommandBuffer(cmd);
            // 释放 Command Buffer
            cmd.Release();
            context.Submit();
        }
    }
}
