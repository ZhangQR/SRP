using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Catlike0201
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
            
            CameraClearFlags flags = camera.clearFlags;

            buffer.ClearRenderTarget(
                flags <= CameraClearFlags.Depth,
                // flags == CameraClearFlags.SolidColor,
                // 感觉这样在 Frame Debugger 里面看要舒服一点
                flags <= CameraClearFlags.SolidColor,
                flags == CameraClearFlags.SolidColor?
                    camera.backgroundColor.linear:Color.clear);

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

