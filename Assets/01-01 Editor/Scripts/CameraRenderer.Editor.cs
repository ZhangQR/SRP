using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Catlike0101
{
    /// <summary>
    /// CameraRenderer 的 editor-only 的内容
    /// </summary>
    public partial class CameraRenderer
    {
        // 同样需要在外部声明
        partial void PrepareForSceneWindow();

        // 同样的，需要在外部声明一下
        partial void DrawGizmos();

        // 因为外部是一直正常使用这个函数，如果不在外部申明一下，有可能会找不到定义
        partial void DrawUnsupportedShaders();

// release 版本不要这些东西
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        
        // 设置一个错误的 Material，也就是玫红色那个
        static Material errorMaterial;

        // build-in 里面的 LightMode，NbSRP 不支持，所以我们让它变成玫红色
        static ShaderTagId[] legacyShaderTagIds = {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
    };

        /// <summary>
        /// 绘制 Gizmos，依然只在编辑器中生效
        /// </summary>
        partial void DrawGizmos()
        {
            if (Handles.ShouldRenderGizmos())
            {
                context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
                context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
            }
        }

        /// <summary>
        /// 绘制 Scene 视图中的 UI
        /// Screen Space - Overlay，UI 是单独绘制的
        /// Screen Space - Camera，UI 是 NbRP 绘制的，在 Transparent 部分
        /// World Space，UI 会变成跟物体一样
        /// </summary>
        partial void PrepareForSceneWindow()
        {
            if (camera.cameraType == CameraType.SceneView)
            {
                ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
            }
        }

        /// <summary>
        /// 绘制那些 NbSRP 不支持的物体
        /// 声明了 partial 的，在每一处都要声明
        /// </summary>
        partial void DrawUnsupportedShaders()
        {
            // 获得玫红色材质球
            if (errorMaterial == null)
            {
                errorMaterial =
                    new Material(Shader.Find("Hidden/InternalErrorShader"));
            }
            
            // 绘制顺序无关紧要
            var drawingSettings = new DrawingSettings(
                legacyShaderTagIds[0], new SortingSettings(camera))
            {
                // 设置覆盖材质，也就是下面的物体都使用这个材质
                overrideMaterial = errorMaterial
            };

            // 将所有的过时着色器添加进去
            for (int i = 1; i < legacyShaderTagIds.Length; i++)
            {
                drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
            }

            // defaultValue 就是不筛选
            var filteringSettings = FilteringSettings.defaultValue;
            
            // 跟上面的一样
            context.DrawRenderers(
                cullingResults, ref drawingSettings, ref filteringSettings
            );
        }
    }

#endif
}

