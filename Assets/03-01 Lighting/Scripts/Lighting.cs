using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

namespace NiuBiSRP
{
    /// <summary>
    /// 传递光源信息给 GPU
    /// </summary>
    public class Lighting
    {
        const string bufferName = "Lighting";

        // 主光源的两个属性
        private static int dirLightColorId = Shader.PropertyToID("_DirectionalLightColor");
        private static int dirLightDirectionId = Shader.PropertyToID("_DirectionalLightDirection");

        CommandBuffer buffer = new CommandBuffer
        {
            name = bufferName
        };


        public void Setup(ScriptableRenderContext context)
        {
            buffer.BeginSample(bufferName);
            SetupDirectionalLight();
            buffer.EndSample(bufferName);
            // 注意这里不执行，直到 submit 才执行
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }
        
        void SetupDirectionalLight () {
            // 可以在 Window / Rendering / Lighting Settings 中设置的，主光源
            Light light = RenderSettings.sun;
            // 颜色要转换成 beta1 空间再做计算，这里要乘上光源强度
            buffer.SetGlobalVector(dirLightColorId, light.color.linear * light.intensity);
            // 传的是 -forword 方向
            buffer.SetGlobalVector(dirLightDirectionId, -light.transform.forward);
        }
    }
}

