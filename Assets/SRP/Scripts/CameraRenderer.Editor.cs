using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace NiuBiSRP
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


        string SampleName { get; set; }

        partial void PrepareBuffer()
        {
            Profiler.BeginSample(camera.name);
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
        const string SampleName = bufferName;
#endif
    }
}

