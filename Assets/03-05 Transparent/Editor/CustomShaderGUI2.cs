using System;
using System.Collections;
using System.Collections.Generic;
using StylizedWater2;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomShaderGUI2 : ShaderGUI
{
    // enum Preset
    // {
    //     Opaque,
    //     Clip,
    //     Fade,
    //     Transparent,
    //     None    // 想要自己设置的话，需要点到 None
    // };
    
    private MaterialEditor materialEditor;
    private Material targetMat;

    private MaterialProperty baseMapProp;
    private MaterialProperty baseColorProp;
    private MaterialProperty srcBlendProp;
    private MaterialProperty dstBlendProp;
    private MaterialProperty zWriteProp;
    private MaterialProperty clippingProp;
    // 因为我们完全使用自定义编辑器了，所以这一项可以不要
    // private MaterialProperty cutoffProp;
    private MaterialProperty metallicProp;
    private MaterialProperty smoothnessProp;
    // private MaterialProperty preMultiplyAlphaProp;
    // private MaterialProperty testProp;

    // KeyWord，因为 preset 需要修改这些值，有 setter 会更方便
    private bool enableClip
    {
        get => targetMat.IsKeywordEnabled("_CLIPPING");
        // set => GUIHelper.SetKeyword(value, "_CLIPPING", targetMat);
        set => CoreUtils.SetKeyword(targetMat, "_CLIPPING", value);
    }

    private bool enablePremultiplayAlpha
    {
        get => targetMat.IsKeywordEnabled("_PREMULTIPLY_ALPHA");
        // set => GUIHelper.SetKeyword(value, "_PREMULTIPLY_ALPHA", targetMat);
        set => CoreUtils.SetKeyword(targetMat, "_PREMULTIPLY_ALPHA", value);
    }

    private bool enableTest
    {
        get => targetMat.IsKeywordEnabled("_TEST");
        // set => GUIHelper.SetKeyword(value, "_TEST", targetMat);
        set => CoreUtils.SetKeyword(targetMat, "_TEST", value);
    }
    
    private UI.Material.Section surfaceOptionSection;
    private UI.Material.Section surfaceInputSection;
    private UI.Material.Section presetSection;
    private UI.Material.Section advanceSection;
    
    private bool initliazed;
    // private Preset preset;

    private void FindProperties(MaterialProperty[] props)
    {
        // 因为每一个都有 getter，所以这里不需要设置了
        // enableClip = targetMat.IsKeywordEnabled("_CLIPPING");
        // enablePremultiplayAlpha = targetMat.IsKeywordEnabled("_PREMULTIPLY_ALPHA");
        // enableTest = targetMat.IsKeywordEnabled("_TEST");

        // if (targetMat.renderQueue < (int)RenderQueue.AlphaTest)
        // {
        //     preset = Preset.Opaque;
        // }else if (targetMat.renderQueue < (int) RenderQueue.Transparent)
        // {
        //     preset = Preset.Clip;
        // }else if (!enablePremultiplayAlpha)
        // {
        //     preset = Preset.Fade;
        // }
        // else
        // {
        //     preset = Preset.Transparent;
        // }
        
        baseMapProp = FindProperty("_BaseMap", props);
        baseColorProp = FindProperty("_BaseColor", props);
        srcBlendProp = FindProperty("_SrcBlend", props);
        dstBlendProp = FindProperty("_DesBlend", props);
        zWriteProp = FindProperty("_ZWrite", props);
        clippingProp = FindProperty("_Clip", props); // value
        // cutoffProp = FindProperty("_CutOff", props); // toggle
        metallicProp = FindProperty("_Metallic", props);
        smoothnessProp = FindProperty("_Smoothness", props);
        // preMultiplyAlphaProp = FindProperty("_PremultiplyAlpha", props);
        // testProp = FindProperty("__", props);
    }

    // 每次点到别的 material editor，再点回来就要调用一次
    void OnEnable()
    {
        surfaceOptionSection =
            new UI.Material.Section(materialEditor, "SURFACE_OPTIONS", new GUIContent("Surface Option"));
        surfaceInputSection = new UI.Material.Section(materialEditor,"SURFACE_INPUT", new GUIContent("Surface Input"));
        presetSection = new UI.Material.Section(materialEditor, "PRESET", new GUIContent("Preset"));
        advanceSection = new UI.Material.Section(materialEditor, "ADVANCE", new GUIContent("Advance"));
        initliazed = true;
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        this.materialEditor = materialEditor;
        this.materialEditor.Repaint();
        
        materialEditor.SetDefaultGUIWidths();
        materialEditor.UseDefaultMargins();
        EditorGUIUtility.labelWidth = 0f;

        targetMat = materialEditor.target as Material;
        if (!initliazed) OnEnable();
        
        FindProperties(props);
        EditorGUI.BeginChangeCheck();
        DrawSurfaceOptions();
        DrawSurfaceInputs();
        DrawPreset();
        DrawAdvance();
        
        if (EditorGUI.EndChangeCheck())
        {
            ApplyChanges();
        }
    }

    private void DrawSurfaceOptions()
    {
        surfaceOptionSection.DrawHeader(()=>SwitchSection(surfaceOptionSection));
        if (EditorGUILayout.BeginFadeGroup(surfaceOptionSection.anim.faded))
        {
            EditorGUILayout.Space();
            
            // 自己写 popup
            var srcBlendMode = (int)srcBlendProp.floatValue;
            srcBlendMode = EditorGUILayout.Popup("Source Blend Mode", srcBlendMode, Enum.GetNames(typeof(UnityEngine.Rendering.BlendMode)));
            srcBlendProp.floatValue = srcBlendMode;
            
            // 省事写 popup
            GUIHelper.DoPopup(new GUIContent("Destinat Blend Mode"),dstBlendProp,Enum.GetNames(typeof(UnityEngine.Rendering.BlendMode)),materialEditor);
            
            // 自定义 Enum 的 Popup
            GUIHelper.DoPopup(new GUIContent("Z Write"),zWriteProp,new string[]{"Off","On"},materialEditor);
            
            // toggle 和其他组合
            // enableClip = UI.Material.Toggle(enableClip, "Clip","Enable Alpha Clip");
            
            enableClip = EditorGUILayout.Toggle(new GUIContent("Clip", null, "Enable Alpha Clip"), enableClip);
            if (enableClip)
            {
                // 写在了 apply change 里面
                // targetMat.EnableKeyword("_CLIPPING");
                float cutoffValue = EditorGUILayout.Slider(new GUIContent("Cutoff Value", "Cutoff Value"),clippingProp.floatValue,0,1);
                clippingProp.floatValue = cutoffValue;
            }
            // else
            // {
            //     targetMat.DisableKeyword("_CLIPPING");
            // }

            // bool cutoff = Mathf.Abs(cutoffProp.floatValue) > float.Epsilon;
            // cutoffProp.floatValue = EditorGUILayout.Toggle(new GUIContent("Clip", null, "Enable Alpha Clip"),
            //     cutoff)
            //     ? 1.0f
            //     : 0.0f;
            // if (cutoff)
            // {
            //     targetMat.EnableKeyword("_CLIPPING");
            //     float cutoffValue = EditorGUILayout.Slider(new GUIContent("Cutoff Value", "Cutoff Value"),clippingProp.floatValue,0,1);
            //     clippingProp.floatValue = cutoffValue;
            // }
            // else
            // {
            //     targetMat.DisableKeyword("_CLIPPING");
            // }
            
            // slide
            float metallic = EditorGUILayout.Slider(new GUIContent("Metallic","金属度"),metallicProp.floatValue, 0, 1);
            metallicProp.floatValue = metallic;
            
            float smoothness = EditorGUILayout.Slider(new GUIContent("Smoothness","光滑度"),smoothnessProp.floatValue, 0, 1);
            smoothnessProp.floatValue = smoothness;

            enablePremultiplayAlpha = EditorGUILayout.Toggle(new GUIContent("Pre Multiply Alpha", "Pre Multiply Alpha"),
                enablePremultiplayAlpha);
            // 写在了 apply change 里面
            // GUIHelper.SetKeyword(enablePremultiplayAlpha, "_PREMULTIPLY_ALPHA", targetMat);
            
            enableTest = EditorGUILayout.Toggle(new GUIContent("Test", "Test"),
                enableTest);
            // GUIHelper.SetKeyword(enableTest, "_TEST", targetMat);

        }
        EditorGUILayout.EndFadeGroup();

    }
    
    private void DrawSurfaceInputs()
    {
        surfaceInputSection.DrawHeader(()=>{SwitchSection(surfaceInputSection);});
        if (EditorGUILayout.BeginFadeGroup(surfaceInputSection.anim.faded))
        {
            EditorGUILayout.Space();
            materialEditor.TexturePropertySingleLine(new GUIContent("Base Map", "Base Map"), baseMapProp,
                baseColorProp);
            materialEditor.TextureScaleOffsetProperty(baseMapProp);
            EditorGUILayout.Space();
        }
        
        EditorGUILayout.EndFadeGroup();
    }

    private void DrawPreset()
    {
        presetSection.DrawHeader(()=>{SwitchSection(presetSection);});
        if (EditorGUILayout.BeginFadeGroup(presetSection.anim.faded))
        {
            EditorGUILayout.Space();
            using (new EditorGUILayout.VerticalScope())
            {
                // 不能用 toolbar
                EditorGUILayout.LabelField("Preset", GUILayout.Width(EditorGUIUtility.labelWidth));
                // preset = (Preset)GUILayout.Toolbar((int)preset,
                //     new GUIContent[] { new GUIContent("Opaque"), new GUIContent("Clip"),
                //         new GUIContent("Fade"),new GUIContent("Transparent")});
                OpaquePreset();
                ClipPreset();
                FadePreset();
                TransparentPreset();
            }

            // switch (preset)
            // {
            //     case Preset.Opaque:
            //         enableClip = false;
            //         enablePremultiplayAlpha = false;
            //         srcBlendProp.floatValue = (float)BlendMode.One;
            //         dstBlendProp.floatValue = (float)BlendMode.Zero;
            //         zWriteProp.floatValue = 1.0f;
            //         targetMat.renderQueue = (int)RenderQueue.Geometry;
            //         break;
            //     case Preset.Clip:
            //         enableClip = true;
            //         enablePremultiplayAlpha = false;
            //         srcBlendProp.floatValue = (float)BlendMode.One;
            //         dstBlendProp.floatValue = (float)BlendMode.Zero;
            //         zWriteProp.floatValue = 1.0f;
            //         targetMat.renderQueue = (int)RenderQueue.AlphaTest;
            //         break;
            //     case Preset.Fade:
            //         enableClip = false;
            //         enablePremultiplayAlpha = false;
            //         srcBlendProp.floatValue = (float)BlendMode.SrcAlpha;
            //         dstBlendProp.floatValue = (float)BlendMode.OneMinusSrcAlpha;
            //         zWriteProp.floatValue = 0.0f;
            //         targetMat.renderQueue = (int)RenderQueue.Transparent;
            //         break;
            //     case Preset.Transparent:
            //         enableClip = false;
            //         enablePremultiplayAlpha = true;
            //         srcBlendProp.floatValue = (float)BlendMode.One;
            //         dstBlendProp.floatValue = (float)BlendMode.OneMinusSrcAlpha;
            //         zWriteProp.floatValue = 0.0f;
            //         targetMat.renderQueue = (int)RenderQueue.Transparent;
            //         break;
            // }
            EditorGUILayout.Space();
        }
        EditorGUILayout.EndFadeGroup();
    }
    
    private bool PressButton(string name)
    {
        if (GUILayout.Button(name))
        {
            // 保存所有 materials 的状态
            materialEditor.RegisterPropertyChangeUndo(name);
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
            enableClip = false;
            enablePremultiplayAlpha = false;
            srcBlendProp.floatValue = (float)BlendMode.One;
            dstBlendProp.floatValue = (float)BlendMode.Zero;
            zWriteProp.floatValue = 1.0f;
            targetMat.renderQueue = (int)RenderQueue.Geometry;
        }
    }

    /// <summary>
    /// 透明度裁剪的预设
    /// </summary>
    private void ClipPreset()
    {
        if (PressButton("Clip"))
        {
            enableClip = true;
            enablePremultiplayAlpha = false;
            srcBlendProp.floatValue = (float)BlendMode.One;
            dstBlendProp.floatValue = (float)BlendMode.Zero;
            zWriteProp.floatValue = 1.0f;
            targetMat.renderQueue = (int)RenderQueue.AlphaTest;
        }
    }

    /// <summary>
    /// 标准的透明模式，高光反射会随着一起 Fade
    /// </summary>
    private void FadePreset()
    {
        if (PressButton("Fade"))
        {
            enableClip = false;
            enablePremultiplayAlpha = false;
            srcBlendProp.floatValue = (float)BlendMode.SrcAlpha;
            dstBlendProp.floatValue = (float)BlendMode.OneMinusSrcAlpha;
            zWriteProp.floatValue = 0.0f;
            targetMat.renderQueue = (int)RenderQueue.Transparent;
        }
    }

    /// <summary>
    /// 预乘 Transparent 预设
    /// </summary>
    private void TransparentPreset()
    {
        if (PressButton("Transparent"))
        {
            enableClip = false;
            enablePremultiplayAlpha = true;
            srcBlendProp.floatValue = (float)BlendMode.One;
            dstBlendProp.floatValue = (float)BlendMode.OneMinusSrcAlpha;
            zWriteProp.floatValue = 0.0f;
            targetMat.renderQueue = (int)RenderQueue.Transparent;
        }
    }
    
    private void DrawAdvance()
    {
        advanceSection.DrawHeader(()=>SwitchSection(advanceSection));
        if (EditorGUILayout.BeginFadeGroup(advanceSection.anim.faded))
        {
            EditorGUILayout.Space();
            materialEditor.EnableInstancingField();
            materialEditor.RenderQueueField();
            EditorGUILayout.Space();
        }
        EditorGUILayout.EndFadeGroup();
    }

    void ApplyChanges()
    {
        CoreUtils.SetKeyword(targetMat,"_CLIPPING",enableClip);
        CoreUtils.SetKeyword(targetMat,"_PREMULTIPLY_ALPHA",enablePremultiplayAlpha);
        CoreUtils.SetKeyword(targetMat,"_Test",enableTest);
        
        EditorUtility.SetDirty(targetMat);
    }
    
    private void SwitchSection(UI.Material.Section s)
    {
        surfaceOptionSection.Expanded = (s == surfaceOptionSection) && !surfaceOptionSection.Expanded;
        surfaceInputSection.Expanded = (s == surfaceInputSection) && !surfaceInputSection.Expanded;
        presetSection.Expanded = (s == presetSection) && !presetSection.Expanded;
        advanceSection.Expanded = (s == advanceSection) && !advanceSection.Expanded;
    }
}
