using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Catlike0102
{
    public partial class CameraRenderer
    {

        ScriptableRenderContext context;
        
        Camera camera;
        
        CullingResults cullingResults;

        const string bufferName = "Render Camera";


        static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");


        CommandBuffer buffer = new CommandBuffer
        {
            name = bufferName
        };


        public void Render(ScriptableRenderContext context, Camera camera)
        {
            
            this.context = context;
            this.camera = camera;

            // 为 Editor 和 Build 准备不同的 Buffer Name
            PrepareBuffer();
            PrepareForSceneWindow();

            if (!Cull())
            {
                return;
            }
            

            Setup();
            DrawVisibleGeometry();

            DrawUnsupportedShaders();

            DrawGizmos();
            Submit();
        }

        void Setup()
        {
            context.SetupCameraProperties(camera);
            
            // 这四种与 Camera 组件上的 4 中一一对应
            CameraClearFlags flags = camera.clearFlags;

            buffer.ClearRenderTarget(
                // 虽然是独立的，但是除了最后一个，都会清除 depth buffer
                // 清除的不只是 depth，还有 stencil
                flags <= CameraClearFlags.Depth,
                // 当是 skybox 的时候，我们不用管，因为后面会画一遍天空盒
                // 注意 DrawSkybox 只有在 flag 为 skybox 的时候才会调用
                // 所以当 Color 时，我们清除 Color Buffer，后面的 DrawSkybox 也不会被调用
                flags == CameraClearFlags.SolidColor,
                //flags == CameraClearFlags.SolidColor?
                //camera.backgroundColor.linear : Color.clear);
                camera.backgroundColor.linear);

            // 换成 SampleName
            buffer.BeginSample(SampleName);
            ExecuteBuffer();

        }

        bool Cull()
        {
            if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
            {
                cullingResults = context.Cull(ref p);
                return true;
            }

            return false;
        }

        void DrawVisibleGeometry()
        {
            var sortingSettings = new SortingSettings(camera)
            {
                criteria = SortingCriteria.CommonOpaque
            };

            var drawingSettings = new DrawingSettings(
                unlitShaderTagId, sortingSettings);
            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

            context.DrawRenderers(
                cullingResults, ref drawingSettings, ref filteringSettings
            );

            // 只有 flag = skybox 的时候才会被调用
            context.DrawSkybox(camera);

            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawingSettings.sortingSettings = sortingSettings;
            filteringSettings.renderQueueRange = RenderQueueRange.transparent;

            context.DrawRenderers(
                cullingResults, ref drawingSettings, ref filteringSettings
            );
        }

        void Submit()
        {
            // 换成 SampleName
            buffer.EndSample(SampleName);
            ExecuteBuffer();
            context.Submit();
        }
        void ExecuteBuffer()
        {
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }
    }
}

