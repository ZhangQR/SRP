using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

/// <summary>
/// 处理 Shadow 相关内容，逻辑上属于 Lighting，由 Lighting 调用
/// </summary>
public class Shadows
{
    const string bufferName = "Shadows";
    private const int maxShadowDirectionalLightCount = 1;
    private int shadowDirectionalLightCount = 0;

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
}
