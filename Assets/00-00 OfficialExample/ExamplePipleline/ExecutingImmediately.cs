using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 展示在 render pipeline 之外立即执行 Command Buffer
/// </summary>
public class ExecutingImmediately : MonoBehaviour
{
    private CommandBuffer commandBuffer;

    private void Awake()
    {
        commandBuffer = new CommandBuffer();
    }
    
    // 命令是将整个屏幕变成青色
    void Start()
    {
        commandBuffer.name = "ExecutingImmediatelyBuffer";
        commandBuffer.SetRenderTarget(RenderTexture.active);
        commandBuffer.ClearRenderTarget(true,true,Color.cyan);
    }
    
    // 没有效果
    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            Graphics.ExecuteCommandBuffer(commandBuffer);
        }   
    }

    // 运行之后会变成青色
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.ExecuteCommandBuffer(commandBuffer);
    }
}
