using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection.Emit;
using UnityEngine;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine.Rendering;

   /*
    * 1、重写 OnGUI，保留原来的 GUI
    * 2、保存三个字段：materialEditor,正在编辑的 Editor；
    * materials,可以多选材质球，相同的 shader 可以一起编辑，MaterialProperty，可用属性
    * 3、封装 SetProperty，SetKeyword，Toggle 版本的 SetProperty
    * 4、封装 Clipping，PremultiplyAlpha ，SrcBlend ，DstBlend ，ZWrite ，RenderQueue ，主要是 setter
    * 5、PressButton，总的预设按钮，包含注册 undo
    * 6、OpaquePreset，ClipPreset ，FadePreset，TransparentPreset
    * 7、在 ONGUI 中绘制这些按钮，并且有 preset 的下拉框
    * 8、适配 Unlit，将 Clip 等属性名在 Unlit 和 Lit 中设置为一样的
    * 9、使找不到属性时不要报错：SetProperty 设置返回值，在找不到时 false，否则 true
    * 10、Unlit 中没有预乘，所以我们在 Unlit 中不要显示 TransparentPreset，增加 HasProperty(string):bool方法，
    *    HasPremultiplyAlpha:bool 属性，并在 TransparentPreset 增加判断
    */

/// <summary>
/// 为 Lit 和 Unlit 定制的 Custom GUI
/// </summary>
public class CustomShaderGUI : ShaderGUI
{
    private MaterialEditor editor;
    private Object[] materials;
    private MaterialProperty[] properties;

    private bool showPreset;
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        base.OnGUI(materialEditor, properties);
        
        editor = materialEditor;
        // 因为相同 shader 的 materials 可以同时编辑，所以这里是一个数组
        materials = materialEditor.targets;
        this.properties = properties;
        
        // 因为预设按钮不会经常使用，所以我们通过 foldout 来折叠
        EditorGUILayout.Space();
        showPreset = EditorGUILayout.Foldout(showPreset, "Presets", true);
        if (showPreset)
        {
            OpaquePreset();
            ClipPreset();
            FadePreset();
            TransparentPreset();
        }
    }

    #region Helper
    
    /// <summary>
    /// 因为 Lit 和 Unlit 共用的这一个 ShaderGUI，所以有些属性 Unlit 是找不到的
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    private bool SetProperty(string name, float value)
    {
        // 这里 false 是为了让找不到的时候，不报错，默认是 true
        var property = FindProperty(name, properties,false);
        if (property != null)
        {
            property.floatValue = value;
            return true;
        }
        return false;
    }

    private void SetKeyword(string name, bool enable)
    {
        // 多想一想再写撒
        // foreach (Material material in materials)
        // {
        //     if (enable)
        //     {
        //         material.EnableKeyword(name);
        //     }
        //     else
        //     {
        //         material.DisableKeyword(name);
        //     }
        // }

        if (enable)
        {
            // (editor.target as Material).EnableKeyword(name);
            foreach (Material material in materials)
            {
                material.EnableKeyword(name);
            }
        }
        else
        {
            // (editor.target as Material).DisableKeyword(name);
            foreach (Material material in materials)
            {
                material.DisableKeyword(name);
            }
        }
    }

    /// <summary>
    /// 使用 Toggle 时可以方便的调用
    /// </summary>
    private void SetProperty(string propertyName,string keyWord,bool enable)
    {
        if (SetProperty(propertyName, enable ? 1.0f : 0.0f))
        {
            SetKeyword(keyWord,enable);
        }
    }
    
    bool HasProperty(string name)=>FindProperty(name, properties, false) != null;
    
    #endregion

    #region Properties
    
    private bool Clipping
    {
        set => SetProperty("_CutOff", "_CLIPPING", value);
    }

    private bool PremultiplyAlpha
    {
        set => SetProperty("_PremultiplyAlpha", "_PREMULTIPLY_ALPHA", value);
    }

    private bool HasPremultiplyAlpha => HasProperty("_PremultiplyAlpha");

    private BlendMode SrcBlend
    {
        set => SetProperty("_SrcBlend", (float) value);
    }
    
    private BlendMode DstBlend
    {
        set => SetProperty("_DesBlend", (float) value);
    }
    
    private bool ZWrite
    {
        set => SetProperty("_ZWrite", value ? 1.0f : 0.0f);
    }

    private RenderQueue RenderQueue
    {
        set
        {
            foreach (Material material in materials)
            {
                material.renderQueue = (int)value;
            }
        }
    }
    #endregion
    
    #region preset

    private bool PressButton(string name)
    {
        if (GUILayout.Button(name))
        {
            // 保存所有 materials 的状态
            editor.RegisterPropertyChangeUndo(name);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Geometry 的预设按钮
    /// </summary>
    private void OpaquePreset()
    {
        if (PressButton("Opaque"))
        {
            Clipping = false;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.Zero;
            ZWrite = true;
            RenderQueue = RenderQueue.Geometry;
        }
    }
    
    /// <summary>
    /// 透明度裁剪的预设
    /// </summary>
    private void ClipPreset()
    {
        if (PressButton("Clip"))
        {
            Clipping = true;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.Zero;
            ZWrite = true;
            RenderQueue = RenderQueue.AlphaTest;
        }
    }
    
    /// <summary>
    /// 标准的透明模式，高光反射会随着一起 Fade
    /// </summary>
    private void FadePreset()
    {
        if (PressButton("Fade"))
        {
            Clipping = false;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.SrcAlpha;
            DstBlend = BlendMode.OneMinusSrcAlpha;
            ZWrite = false;
            RenderQueue = RenderQueue.Transparent;
        }
    }
    
    /// <summary>
    /// 预乘 Transparent 预设
    /// </summary>
    private void TransparentPreset()
    {
        // 因为 Unlit 中没有 premultiplyAlpha，所以不显示
        if (HasPremultiplyAlpha && PressButton("Transparent"))
        {
            Clipping = false;
            PremultiplyAlpha = true;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.OneMinusSrcAlpha;
            ZWrite = false;
            RenderQueue = RenderQueue.Transparent;
        }
    }
    #endregion
    
}
