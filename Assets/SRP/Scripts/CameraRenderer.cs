using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
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


        public void Render(ScriptableRenderContext context, Camera camera,bool useDynamicBatching,bool useGPUInstance,ShadowSetting shadow)
        {
            this.context = context;
            this.camera = camera;

            PrepareBuffer();
            PrepareForSceneWindow();

            if (!Cull(shadow.maxDistance))
            {
                return;
            }
            
            buffer.BeginSample(SampleName);
            // 一般来说 BeginSample 后面要紧挨着一句 execute
            ExecuteBuffer();
            // 1、设置好灯光信息 2、绘制好 Shadow Map
            lighting.Setup(context,cullingResults,shadow);
            buffer.EndSample(SampleName);
            

            // 因为在 Lighting 中要设置 VP 和 Render Target，所以要将我们的相机设置移到后面
            // 不然 object 会绘制到 shadow map 上，报错 Dimensions of color surface does not match dimensions of depth
            Setup();
            
            DrawVisibleGeometry(useDynamicBatching,useGPUInstance);

            DrawUnsupportedShaders();

            DrawGizmos();
            // 在提交之前释放 Shadow Map 的 RT
            lighting.CleanUp();
            Submit();
        }

        void Setup()
        {
            // 之前的 RT 是 shadow map,极有可能是在这里变回了 camera
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
        
        bool Cull(float maxDistance)
        {
            if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
            {
                // 设置阴影的最大距离
                p.shadowDistance = Mathf.Min(camera.farClipPlane, maxDistance);
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

