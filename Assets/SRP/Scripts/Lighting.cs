using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace NiuBiSRP
{
    /// <summary>
    /// 传递光源信息给 GPU
    /// </summary>
    public class Lighting
    {
        const string bufferName = "Lighting";

        // 方向光的两个属性，颜色和方向
        // 方向光的最大数量（也就是在 shader 中数组开辟的空间（正），超了可能会原地爆炸（误）
        // 方向光的实际数量，因为我们在 shader 中索引的时候，是索引的实际灯光数。
        // 比如实际有 3 个，就不能去获得第 4 个的灯光信息
        private const int maxDirectionalLightCount = 4;
        private static int dirLightColorId = Shader.PropertyToID("_DirectionalLightColors");
        private static int dirLightDirectionId = Shader.PropertyToID("_DirectionalLightDirections");
        private static int dirLightCountId = Shader.PropertyToID("_DirectionalLightCount");
        private static int dirLightShadowDataId = Shader.PropertyToID("_DirectionalLightShadowData"); 
        
        private static Vector4[] dirLightColors = new Vector4[maxDirectionalLightCount];
        private static Vector4[] dirLightDirections = new Vector4[maxDirectionalLightCount];
        private static Vector4[] dirLightShadowData = new Vector4[maxDirectionalLightCount];
        
        private CullingResults cullingResults;
        

        CommandBuffer buffer = new CommandBuffer
        {
            name = bufferName
        };

        private Shadows shadows = new Shadows();


        public void Setup(ScriptableRenderContext context,CullingResults cullingResults,ShadowSetting shadowSetting)
        {
            this.cullingResults = cullingResults;
            buffer.BeginSample(bufferName);
            shadows.Setup(context,cullingResults,shadowSetting);
            SetupLights();
            
            // 是 Shadow 的 Render，却是 Lighting 的 Setup
            shadows.Render();
            
            buffer.EndSample(bufferName);
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }

        /// <summary>
        /// 遍历所有可见灯光，然后按照类型填充上面的数组，并且将填充过的数组传给 shader
        /// </summary>
        private void SetupLights()
        {
            var visibleLights = cullingResults.visibleLights;
            int length = visibleLights.Length;
            int dirLightCount = 0;
            // 遍历可见光，超过最大灯光数量时停止
            for (int i = 0; i < length; i++)
            {
                var light = visibleLights[i];
                if (light.lightType == LightType.Directional && 
                    dirLightCount < maxDirectionalLightCount)
                {
                    SetupDirectionalLight(dirLightCount++,ref light);
                }
            }

            // dirLightColors[1] = new Vector4(0, 1, 0, 0);
            // dirLightDirections[1] = Vector4.one;
            //
            // dirLightColors[2] = new Vector4(1, 0, 0, 0);
            // dirLightDirections[2] = new Vector4(0,1,0,0);

            // 将填充完成的数组放到 shader 全局变量中
            buffer.SetGlobalInt(dirLightCountId,length);
            buffer.SetGlobalVectorArray(dirLightColorId,dirLightColors);
            buffer.SetGlobalVectorArray(dirLightDirectionId,dirLightDirections);
            buffer.SetGlobalVectorArray(dirLightShadowDataId,dirLightShadowData);;
        }

        private void SetupDirectionalLight(int index, ref VisibleLight light)
        {
            // 我们需要在 pipeline 的构造函数中将颜色空间设置为线性
            dirLightColors[index] = light.finalColor;
            // localToWorld 的矩阵就是 xyz 三个方向和 o 点坐标，按列顺着摆放
            // 所以第三列就是 z 方向
            // 同样的，我们传过去 -z 方向
            dirLightDirections[index] = -light.localToWorldMatrix.GetColumn(2);
            
            dirLightShadowData[index] = shadows.ReserveDirectionalShadows(light.light,index);
        }

        public void CleanUp()
        {
            shadows.CleanUp();
        }
        
    }
}

