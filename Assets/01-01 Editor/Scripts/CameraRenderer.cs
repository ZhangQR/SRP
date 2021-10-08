using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Catlike0101
{
    /// <summary>
    /// CameraRenderer 的主体部分，也就是剔除 editor-only 的部分
    /// </summary>
    public partial class CameraRenderer
    {

        ScriptableRenderContext context;
        
        Camera camera;
        
        CullingResults cullingResults;

        const string bufferName = "Render Camera";


        static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");


        // editor-only 的内容全部提到外面来写
    //    // 设置一个错误的 Material，也就是玫红色那个
    //    static Material errorMaterial;

    //    // build-in 里面的 LightMode，NbSRP 不支持，所以我们让它变成玫红色
    //    static ShaderTagId[] legacyShaderTagIds = {
    //    new ShaderTagId("Always"),
    //    new ShaderTagId("ForwardBase"),
    //    new ShaderTagId("PrepassBase"),
    //    new ShaderTagId("Vertex"),
    //    new ShaderTagId("VertexLMRGBM"),
    //    new ShaderTagId("VertexLM")
    //};

        CommandBuffer buffer = new CommandBuffer
        {
            name = bufferName
        };


        public void Render(ScriptableRenderContext context, Camera camera)
        {
            this.context = context;
            this.camera = camera;
            
            // 如果写在 Cull 后面，在场景视图中绘制不出 UI
            PrepareForSceneWindow();

            if (!Cull())
            {
                return;
            }
            

            Setup();
            DrawVisibleGeometry();

            // 在画完可见物体之后，绘制玫红色，注意：会覆盖透明物体
            // 这里可以正常使用它，并且不在编辑器时也不会报错，就是因为我们在外部有一个空的函数声明
            DrawUnsupportedShaders();

            // 绘制 Gizmos，只在编辑器中生效
            DrawGizmos();
            Submit();
        }

        void Setup()
        {
            context.SetupCameraProperties(camera);
            buffer.ClearRenderTarget(true, true, Color.clear);
            buffer.BeginSample(bufferName);
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

        // editor-only 的内容全部提到外面来写

        ///// <summary>
        ///// 绘制那些 NbSRP 不支持的物体
        ///// </summary>
        //void DrawUnsupportedShaders()
        //{
        //    // 获得玫红色材质球
        //    if (errorMaterial == null)
        //    {
        //        errorMaterial =
        //            new Material(Shader.Find("Hidden/InternalErrorShader"));
        //    }
            
        //    // 绘制顺序无关紧要
        //    var drawingSettings = new DrawingSettings(
        //        legacyShaderTagIds[0], new SortingSettings(camera))
        //    {
        //        // 设置覆盖材质，也就是下面的物体都使用这个材质
        //        overrideMaterial = errorMaterial
        //    };

        //    // 将所有的过时着色器添加进去
        //    for (int i = 1; i < legacyShaderTagIds.Length; i++)
        //    {
        //        drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
        //    }

        //    // defaultValue 就是不筛选
        //    var filteringSettings = FilteringSettings.defaultValue;
            
        //    // 跟上面的一样
        //    context.DrawRenderers(
        //        cullingResults, ref drawingSettings, ref filteringSettings
        //    );
        //}

        void Submit()
        {
            buffer.EndSample(bufferName);
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

