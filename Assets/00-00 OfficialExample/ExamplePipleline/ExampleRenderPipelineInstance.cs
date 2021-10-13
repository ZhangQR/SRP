using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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
        cmd.ClearRenderTarget(true, true, renderPipelineAsset.exampleColor);
        // 把命令添加进 context，在没有 submit 之前是不会被执行的
        context.ExecuteCommandBuffer(cmd);
        // 释放 Command Buffer
        cmd.Release();
        

        
        // 遍历所有的相机
        foreach (Camera camera in cameras)
        {
            // ********************************************************************************
            // ********************************************************************************
            // ****    Cull  :筛选相机外的东西                                                ****
            // ****    1、获取 culling parameters，得到 ScriptableCullingParameters          ****
            // ****    2、（可选）手动更新 ScriptableCullingParameters                        ****
            // ****    3、使用 culling parameters 来 cull，并且保存 Cull 结果                  ****
            // ********************************************************************************
            // ********************************************************************************
            
            // 获取 culling parameters
            camera.TryGetCullingParameters(out var cullingParameters);

            // 使用 culling parameters 来 cull，并且保存 Cull 结果
            var cullingResults = context.Cull(ref cullingParameters);


            // ********************************************************************************
            // ********************************************************************************
            // ****    Draw  :用指定的设置来绘制制定的几何体                                     ****
            // ****    1、获得 cullingResults                                               ****
            // ****    2、创建 FilteringSettings，用来设置如何处理 cullingResults               ****
            // ****    3、创建 DrawingSettings，用来设置绘制哪些几何体，和如何绘制几何体            ****
            // ****    4、（可选）Unity 有默认的渲染状态，如果你想更改，你可以使用 RenderStateBlock  ****
            // ****    5、调用 ScriptableRenderContext.DrawRenderers，并且把刚刚那一堆设置传进去 ****
            // ********************************************************************************
            // ********************************************************************************
            
            // Update the value of built-in shader variables, based on the current Camera
            context.SetupCameraProperties(camera);
            
            // 告诉 Unity 要绘制哪一种几何体，根据 Shader 中的 LightMode Tag
            ShaderTagId shaderTagId = new ShaderTagId("ExampleLightModeTag");
            
            // 告诉 Unity 几何体如何排序，根据每个相机
            var sortingSettings = new SortingSettings(camera);

            // 创建 DrawingSettings，用来设置绘制哪些几何体，和如何绘制几何体
            DrawingSettings drawingSettings = new DrawingSettings(shaderTagId, sortingSettings);
            
            // 告诉 Unity 如何筛选剔除结果，来进一步指定绘制哪些几何体；FilteringSettings.defaultValue 不进行筛选
            FilteringSettings filteringSettings = FilteringSettings.defaultValue;
            
            // 根据我们之前的设置，安排一个命令进行绘制
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

            // 如果需要的话，安排一个命令来绘制天空盒
            if (camera.clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null)
            {
                context.DrawSkybox(camera);
            }

            // 二、直接使用 context 的命令，比如 
            // ScriptableRenderContext.Cull 或者 ScriptableRenderContext.DrawRenderers
            // context.DrawSkybox(cameras[0]);

            // 三、Graphics.ExecuteCommandBuffer 可以直接执行 Command Buffer，不需要 SRP

            // 不管是哪种方式，在没有 Submit 之前都不会执行
            context.Submit();
        }
    }
}
