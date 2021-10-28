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
    private const int maxShadowDirectionalLightCount = 4;
    private int shadowDirectionalLightCount = 0;

    private static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");

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
    /// <param name="cull"></param>
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
    public void ReserveDirectionalShadows(Light light,int visibleLightId)
    {
        if (shadowDirectionalLightCount < maxShadowDirectionalLightCount &&
            light.shadowStrength > 0 && light.shadows != LightShadows.None &&
            // 这个参数没明白，visibleLightId 实际上是 Directional Light Id
            cullingResults.GetShadowCasterBounds(visibleLightId,out Bounds bound))
        {
            // shadowDirectionalLightCount 是 shadow array 的 id，
            // visibleLightId 是 Directional Light Array 的 Id
            shadowDirectionalLights[shadowDirectionalLightCount++] =
                new ShadowDirectionalLight
                {
                    visibleLightId = visibleLightId
                };
        }
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
        
        int split = shadowDirectionalLightCount <= 1 ? 1 : 2;
        int tileSize = atlasSize / split;
        for(int i = 0;i<shadowDirectionalLightCount;i++)
        {
            var light = shadowDirectionalLights[i];
            RenderDirecitonalShadows(i,split,tileSize);
            
            // 注意 setviewport 的顺序，至少要在设置 VP 之前
            // SetViewport(split,i,tileSize);
        }
        buffer.EndSample(bufferName);
        ExecuteBuffer();
    }

    private void SetViewport(int split,int index,int tileSize)
    {
        Vector2 offset = new Vector2(index % split, index / split);
        offset *= tileSize;
        buffer.SetViewport(new Rect(offset.x,offset.y,tileSize,tileSize));
    }

    private void RenderDirecitonalShadows(int index,int split,int tileSize)
    {
        var light = shadowDirectionalLights[index];
        ShadowDrawingSettings shadowDrawingSettings = new ShadowDrawingSettings(cullingResults, light.visibleLightId);
        cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
            light.visibleLightId, 0, 1, Vector3.zero,
            tileSize, 0, out Matrix4x4 viewMatrix,
            out Matrix4x4 projMatrix, out ShadowSplitData shadowSplitData);
        shadowDrawingSettings.splitData = shadowSplitData;
        SetViewport(split, index, tileSize);
        buffer.SetViewProjectionMatrices(viewMatrix,projMatrix);
        ExecuteBuffer();
        context.DrawShadows(ref shadowDrawingSettings);
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
