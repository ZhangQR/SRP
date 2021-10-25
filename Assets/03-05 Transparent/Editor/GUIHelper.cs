using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public static class GUIHelper
{
    public static void DoPopup(GUIContent label, MaterialProperty property, string[] options, MaterialEditor materialEditor)
    {
        if (property == null)
            throw new ArgumentNullException("property");

        EditorGUI.showMixedValue = property.hasMixedValue;

        var mode = property.floatValue;
        EditorGUI.BeginChangeCheck();
        mode = EditorGUILayout.Popup(label, (int)mode, options);
        if (EditorGUI.EndChangeCheck())
        {
            materialEditor.RegisterPropertyChangeUndo(label.text);
            property.floatValue = mode;
        }

        EditorGUI.showMixedValue = false;
    }

    // 使用 CoreUtils.SetKeyword
    // public static void SetKeyword(bool enable, string keyword, in Material material)
    // {
    //     if (enable)
    //     {
    //         material.EnableKeyword(keyword);
    //     }
    //     else
    //     {
    //         material.DisableKeyword(keyword);
    //     }
    // }
    
}
