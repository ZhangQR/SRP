using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

/// <summary>
/// 处理 Shadow 相关内容，逻辑上属于 Lighting，由 Lighting 调用
/// </summary>
public class Shadows
{
    const string bufferName = "Shadows";
    private const int maxShadowDirectionalLightCount = 4,maxCascades = 4;
    private int shadowDirectionalLightCount = 0;

    private static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas"),
        dirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices"),
        cascadeCountId = Shader.PropertyToID("_CascadeCount"),
        cascadeCullSpheresId = Shader.PropertyToID("_CascadeCullSpheres"),
        
        // shadow 的最大距离，view space depth
        shadowDistanceId = Shader.PropertyToID("_ShaderDistance");

    private Matrix4x4[] dirShadowMatrices = new Matrix4x4[maxShadowDirectionalLightCount * maxCascades];
    // 每个灯光都可以使用一套 cullSphere
    private Vector4[] cascadeCullSpheres = new Vector4[maxCascades];

    struct ShadowDirectionalLight
    {
        public int visibleLightId;
    }

    private ShadowDirectionalLight[] shadowDirectionalLights =
        new ShadowDirectionalLight[maxShadowDirectionalLightCount];
    
    private CullingResults cullingResults;
    private ScriptableRenderContext context;
    private ShadowSetting setting;
    CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };

    /// <summary>
    /// 保留传过来的参数，在 Lighting 的 setup 之前
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cull"></para   m>
    /// <param name="shadowSetting"></param>
    public void Setup(ScriptableRenderContext context,CullingResults cull,
        ShadowSetting shadowSetting)
    {
        this.context = context;
        cullingResults = cull;
        setting = shadowSetting;

        shadowDirectionalLightCount = 0;
    }

    /// <summary>
    /// 填充 shadowDirectionalLights，在 Light 遍历 Directional Light 的时候调用
    /// </summary>
    /// <param name="light"></param>
    /// <param name="visibleLightId"></param>
    public Vector2 ReserveDirectionalShadows(Light light,int visibleLightId)
    {
        if (shadowDirectionalLightCount < maxShadowDirectionalLightCount &&
            light.shadowStrength > 0 && light.shadows != LightShadows.None &&
            // 这个参数没明白，visibleLightId 实际上是 Directional Light Id
            cullingResults.GetShadowCasterBounds(visibleLightId,out Bounds bound))
        {
            // shadowDirectionalLightCount 是 shadow array 的 id，
            // visibleLightId 是 Directional Light Array 的 Id
            shadowDirectionalLights[shadowDirectionalLightCount] =
                new ShadowDirectionalLight
                {
                    visibleLightId = visibleLightId
                };
            return new Vector2(light.shadowStrength, setting.directional.CascadeCount * shadowDirectionalLightCount++);
        }
        return Vector2.zero;
    }

    /// <summary>
    /// 将 buffer 放到 context 里面，并且清除
    /// </summary>
    private void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    /// <summary>
    /// 从灯光出发，将物体的深度值渲染到 RT 中，是 Shadow 的 render，却是 Lighting 的 Setup
    /// </summary>
    public void Render()
    {
        if (shadowDirectionalLightCount > 0)
        {
            RenderDirectionalShadows();
        }
        else
        {
            // 当不需要绘制阴影的时候，我们仍需要创建一个很小的 RT，depth 可以设置为 16,24,32,
            buffer.GetTemporaryRT(dirShadowAtlasId,1,1,32, 
                FilterMode.Bilinear,RenderTextureFormat.Shadowmap);
            ExecuteBuffer();
        }
    }

    private void RenderDirectionalShadows()
    { 
        int atlasSize = (int)setting.directional.altasSize;
        buffer.GetTemporaryRT(dirShadowAtlasId,atlasSize,atlasSize,32, 
            FilterMode.Bilinear,RenderTextureFormat.Shadowmap);
        buffer.SetRenderTarget(dirShadowAtlasId,
            RenderBufferLoadAction.DontCare,RenderBufferStoreAction.Store);
        buffer.ClearRenderTarget(true,false,Color.clear);
        buffer.BeginSample(bufferName);
        ExecuteBuffer();
        
        // 只支持 1,2,4，即只能分 1,4,16 块
        int tiles = shadowDirectionalLightCount * setting.directional.CascadeCount;
        int split = tiles <= 1 ? 1 : tiles <= 4 ? 2 : 4;
        int tileSize = atlasSize / split;
        for(int i = 0;i<shadowDirectionalLightCount;i++)
        {
            RenderDirecitonalShadows(i,split,tileSize);
        }
        buffer.SetGlobalInt(cascadeCountId,setting.directional.CascadeCount);
        buffer.SetGlobalVectorArray(cascadeCullSpheresId,cascadeCullSpheres);
        buffer.SetGlobalMatrixArray(dirShadowMatricesId,dirShadowMatrices);
        buffer.SetGlobalFloat(shadowDistanceId,setting.maxDistance);
        buffer.EndSample(bufferName);
        ExecuteBuffer();
    }

    private Vector2 SetViewport(int split,int index,int tileSize)
    {
        Vector2 offset = new Vector2(index % split, index / split);
        buffer.SetViewport(new Rect(offset.x * tileSize,offset.y * tileSize,tileSize,tileSize));
        return offset;
    }

    private void RenderDirecitonalShadows(int index,int split,int tileSize)
    {
        var light = shadowDirectionalLights[index];
        ShadowDrawingSettings shadowDrawingSettings = new ShadowDrawingSettings(cullingResults, light.visibleLightId);
        int cascadeCount = setting.directional.CascadeCount;
        int tileOffset = index * cascadeCount;
        Vector3 ratios = setting.directional.CascadeRatios;
        for (int i = 0; i < cascadeCount; i++)
        {
            cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
                light.visibleLightId, i, cascadeCount, ratios,
                tileSize, 0, out Matrix4x4 viewMatrix,
                out Matrix4x4 projMatrix, out ShadowSplitData shadowSplitData);
            shadowDrawingSettings.splitData = shadowSplitData;
            int tileIndex = tileOffset + i;
            dirShadowMatrices[tileIndex] = ConvertToAtlasMatrix(projMatrix * viewMatrix,
                SetViewport(split, tileIndex, tileSize), split);
            buffer.SetViewProjectionMatrices(viewMatrix, projMatrix);
            if (index == 0)
            {
                var cullingsphere = shadowSplitData.cullingSphere;
                // 因为在 shader 中只要用到平方
                cullingsphere.w *= cullingsphere.w;
                cascadeCullSpheres[i] = cullingsphere;
            }
            ExecuteBuffer();
            context.DrawShadows(ref shadowDrawingSettings);
        }
    }

    /// <summary>
    /// 将 vp 矩阵处理成符合分块的形式
    /// </summary>
    /// <param name="vpMatrix"></param>
    /// <param name="offset"></param>
    /// <param name="split"></param>
    /// <returns></returns>
    private Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m, Vector2 offset, int split)
    {
        if (SystemInfo.usesReversedZBuffer)
        {
            m.m20 = -m.m20;
            m.m21 = -m.m21;
            m.m22 = -m.m22;
            m.m23 = -m.m23;
        }
        // 将 [-1,1] 转换到 [0,1]
        m.m00 = 0.5f * (m.m00 + m.m30);
        m.m01 = 0.5f * (m.m01 + m.m31);
        m.m02 = 0.5f * (m.m02 + m.m32);
        m.m03 = 0.5f * (m.m03 + m.m33);
        m.m10 = 0.5f * (m.m10 + m.m30);
        m.m11 = 0.5f * (m.m11 + m.m31);
        m.m12 = 0.5f * (m.m12 + m.m32);
        m.m13 = 0.5f * (m.m13 + m.m33);
        m.m20 = 0.5f * (m.m20 + m.m30);
        m.m21 = 0.5f * (m.m21 + m.m31);
        m.m22 = 0.5f * (m.m22 + m.m32);
        m.m23 = 0.5f * (m.m23 + m.m33);
        
        // 进行分块
        float scale = 1f / split;
        m.m00 = (m.m00 + offset.x * m.m30) * scale;
        m.m01 = (m.m01 + offset.x * m.m31) * scale;
        m.m02 = (m.m02 + offset.x * m.m32) * scale;
        m.m03 = (m.m03 + offset.x * m.m33) * scale;
        m.m10 = (m.m10 + offset.y * m.m30) * scale;
        m.m11 = (m.m11 + offset.y * m.m31) * scale;
        m.m12 = (m.m12 + offset.y * m.m32) * scale;
        m.m13 = (m.m13 + offset.y * m.m33) * scale;
        
        // 上面两步可以合在一起写：
        // float scale = 1f / split;
        // m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
        // m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
        // m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
        // m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
        // m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
        // m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
        // m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
        // m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
        
        return m;
    }

    /// <summary>
    /// 清除 buffer 和 RT
    /// </summary>
    public void CleanUp()
    {
        buffer.ReleaseTemporaryRT(dirShadowAtlasId);
        ExecuteBuffer();
    }

}
