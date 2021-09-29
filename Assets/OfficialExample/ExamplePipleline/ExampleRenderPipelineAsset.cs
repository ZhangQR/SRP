using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "CustomRenderPipeline/ExampleRenderPipelineAsset")]
public class ExampleRenderPipelineAsset:RenderPipelineAsset
{
    // ��Щ���Իᱻ��ʾ�� Inspector ��
    public Color exampleColor;
    public string exampleString;
    
    // Unity �ڻ���ÿһ֮֡ǰ����������������б任���ʹݻٵ�ǰ�� Pipeline���������´���һ��
    protected override RenderPipeline CreatePipeline()
    {
        // ʵ����һ�� Render Pipeline �� SRP ��Ⱦ�����Ҵ��� Asset ������
        // Render Pipeline ���Զ�ȡ Asset ������
        return new ExampleRenderPipelineInstance(this);
    }
}
