using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 实现毛玻璃效果,https://docs.unity3d.com/Manual/GraphicsCommandBuffers.html
/// </summary>
[ExecuteInEditMode]
public class CommandBufferBlurRefraction : MonoBehaviour
{
    public Shader blurShader;
    private Material material;

    // 因为希望不止一个相机渲染毛玻璃效果，所以使用一个字典
    private Dictionary<Camera, CommandBuffer> cameras =
        new Dictionary<Camera, CommandBuffer>();

    // 移除所有的 CommandBuffer
    private void CleanUp()
    {
        foreach(var cam in cameras)
        {
            if (cam.Key)
            {
                cam.Key.RemoveCommandBuffer(CameraEvent.AfterSkybox, cam.Value);
            }
        }
        cameras.Clear();
        Object.DestroyImmediate(material);
    }

    public void OnEnable()
    {
        CleanUp();
    }

    public void OnDisable()
    {
        CleanUp();
    }

    // 如果对象可见并且不是 UI 元素，则为每个摄像机调用 OnWillRenderObject。
    private void OnWillRenderObject()
    {
        // Debug.Log(gameObject.name + " OnWillRenderObject");
        var act = gameObject.activeInHierarchy && enabled;
        if (!act)
        {
            CleanUp();
            return;
        }

        var cam = Camera.current;
        if (!cam)
        {
            return;
        }

        CommandBuffer buf = null;
        // 如果 cameras 已经包含了当前相机，说明 command 已经被注册过了
        // 所以不做任何处理
        if (cameras.ContainsKey(cam))
        {
            return;
        }

        if (!material)
        {
            material = new Material(blurShader);
            material.hideFlags = HideFlags.HideAndDontSave;
        }

        buf = new CommandBuffer();
        buf.name = "Grab screen and blur";
        cameras[cam] = buf;

        // 将屏幕拷贝到临时 RT
        int screenCopyID = Shader.PropertyToID("_ScreenCopyTexture");
        // -1，-1 表示相机像素，深度可以设置为 0,16,24
        buf.GetTemporaryRT(screenCopyID, -1, -1, 0, FilterMode.Bilinear);
        buf.Blit(BuiltinRenderTextureType.CurrentActive, screenCopyID);

        // 获取两个更小的 RT
        int blurredID = Shader.PropertyToID("_Temp1");
        int blurredID2 = Shader.PropertyToID("_Temp2");
        // -2，-2 是相机像素的二分之一
        buf.GetTemporaryRT(blurredID, -2, -2, 0, FilterMode.Bilinear);
        buf.GetTemporaryRT(blurredID2, -2, -2, 0, FilterMode.Bilinear);

        // 将屏幕信息降采样到更小的 RT，并且释放屏幕 RT
        buf.Blit(screenCopyID, blurredID);
        buf.ReleaseTemporaryRT(screenCopyID);

        // 在不同方向进行模型，并且在两个 RT 之间反复横跳
        // 水平模糊,两个纹素大小
        buf.SetGlobalVector("offsets", new Vector4(2.0f/Screen.width, 0, 0, 0));
        buf.Blit(blurredID, blurredID2, material);

        // 垂直模糊
        buf.SetGlobalVector("offsets", new Vector4(0, 2.0f/Screen.height, 0, 0));
        buf.Blit(blurredID2, blurredID, material);

        // 偏移更大的水平模糊，4个纹素大小
        buf.SetGlobalVector("offsets", new Vector4(4.0f / Screen.width, 0, 0, 0));
        buf.Blit(blurredID, blurredID2, material);

        // 偏移更大的垂直模糊
        buf.SetGlobalVector("offsets", new Vector4(0, 4.0f / Screen.height, 0, 0));
        buf.Blit(blurredID2, blurredID, material);

        // 看一眼模糊成什么亚子了~
        // buf.Blit(blurredID, BuiltinRenderTextureType.CameraTarget);

        // 将模糊后的图片传给 _GrabBlurTexture
        buf.SetGlobalTexture("_GrabBlurTexture", blurredID);

        // 前向渲染中，在 opaque 之后渲染 skybox 
        // https://docs.unity3d.com/Manual/GraphicsCommandBuffers.html#order-of-execution
        cam.AddCommandBuffer(CameraEvent.AfterSkybox, buf);

    }
}
