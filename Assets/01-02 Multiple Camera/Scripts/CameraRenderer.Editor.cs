using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace Catlike0102
{
    public partial class CameraRenderer
    {
        partial void PrepareBuffer();
        partial void PrepareForSceneWindow();

        partial void DrawGizmos();

        partial void DrawUnsupportedShaders();


#if UNITY_EDITOR || DEVELOPMENT_BUILD

        static Material errorMaterial;

        static ShaderTagId[] legacyShaderTagIds = {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
    };


        // 编辑器模式下，是一个属性，为相机名称
        string SampleName { get; set; }
        // const string SampleName = bufferName;

        /// <summary>
        /// 1、我们希望能在 Frame Debugger 中将不同相机分开来，所以需要给每个 Buffer 设置成当前相机的名字
        /// 2、但是当 BeginSample 中的参数跟 Buffer Name 不一致的时候，打开 Profiler 会报错
        /// 3、如果只是简单的将 BeginSample 设置成 Buffer Name，那么会频繁调用 GC 分配，在 Build 中我们不希望有这样的开销
        /// 4、所以需要在 Editor 让 Sample 和 BufferName 一致，为 Camera Name
        /// 5、但是在 Build 中所有的 Sample 和 BufferName 都是一个 常量字符串
        /// </summary>
        partial void PrepareBuffer()
        {
            // 使用 Profiler.BeginSample 可以很方便的在 Profiler 中分层
            // 这些运算是在编辑器中独有的，所以单独分一层
            Profiler.BeginSample(camera.name);
            //string name = camera.name;
            //buffer.name = name;
            //SampleName = name;
            // 两个相机的名字分别为“Main Camera”“Second Camera”，一共 24 个字母，48 字节
            // 但是 Profiler 显示的是 100 字节，还有 52 字节暂时不知道从何而来(实际上是 26+26)
            // 除了这里的 100 字节，loop 中还有一部分 Unity 分配的内存，用来存储相机的矩阵等信息
            // 多一个相机多 8 字节，一个相机时是 40 字节，两个为 48 字节。
            buffer.name = SampleName = camera.name;
            Profiler.EndSample();
        }


        partial void DrawGizmos()
        {
            if (Handles.ShouldRenderGizmos())
            {
                context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
                context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
            }
        }
        partial void PrepareForSceneWindow()
        {
            if (camera.cameraType == CameraType.SceneView)
            {
                ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
            }
        }

        partial void DrawUnsupportedShaders()
        {
            if (errorMaterial == null)
            {
                errorMaterial =
                    new Material(Shader.Find("Hidden/InternalErrorShader"));
            }
            
            var drawingSettings = new DrawingSettings(
                legacyShaderTagIds[0], new SortingSettings(camera))
            {
                overrideMaterial = errorMaterial
            };

            for (int i = 1; i < legacyShaderTagIds.Length; i++)
            {
                drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
            }

            var filteringSettings = FilteringSettings.defaultValue;
            
            context.DrawRenderers(
                cullingResults, ref drawingSettings, ref filteringSettings
            );
        }
#else
        // 如果是 build 就为常量
        const string SampleName = bufferName;
#endif
    }
}

