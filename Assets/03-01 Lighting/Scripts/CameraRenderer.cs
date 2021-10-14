using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace NiuBiSRP
{
    public partial class CameraRenderer
    {

        ScriptableRenderContext context;
        
        Camera camera;
        
        CullingResults cullingResults;

        const string bufferName = "Render Camera";


        static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
        static ShaderTagId litShaderTagId = new ShaderTagId("NiuBiLit");


        CommandBuffer buffer = new CommandBuffer
        {
            name = bufferName
        };

        private Lighting lighting = new Lighting();


        public void Render(ScriptableRenderContext context, Camera camera,bool useDynamicBatching,bool useGPUInstance)
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
            
            // 要在设置好相机，但没绘制物体之前设置 lighting
            lighting.Setup(context);
            
            DrawVisibleGeometry(useDynamicBatching,useGPUInstance);

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

        void DrawVisibleGeometry(bool useDynamicBatching,bool useGPUInstance)
        {
            var sortingSettings = new SortingSettings(camera)
            {
                criteria = SortingCriteria.CommonOpaque
            };

            var drawingSettings = new DrawingSettings(
                unlitShaderTagId, sortingSettings)
            {
                enableDynamicBatching = useDynamicBatching,
                enableInstancing = useGPUInstance
            };
            // 跟 DrawUnsupportedShaders 里的一样，0 是 unlitShaderTagId，所以这里从 1 开始
            drawingSettings.SetShaderPassName(1,litShaderTagId);
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

