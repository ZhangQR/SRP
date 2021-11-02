using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NiuBiSRP
{
    public class PerObjectMaterialProperties : MonoBehaviour
    {
        [SerializeField] Color baseColor;
        [Range(0, 1),SerializeField] private float metallic;
        [Range(0, 1),SerializeField] private float smoothness;
        [Range(0, 1),SerializeField] private float clip;
        static int baseColorId = Shader.PropertyToID("_BaseColor");
        static int metallicId = Shader.PropertyToID("_Metallic");
        static int smoothnessId = Shader.PropertyToID("_Smoothness");
        static int clipId = Shader.PropertyToID("_Clip");
        
        static MaterialPropertyBlock block;
        
        private void Awake()
        {
            // baseColor = new Color(Random.value, Random.value, Random.value);
            OnValidate();
        }
        
        void OnValidate()
        {
            if (block == null)
            {
                block = new MaterialPropertyBlock();
            }
            block.SetColor(baseColorId,baseColor);
            block.SetFloat(metallicId,metallic);
            block.SetFloat(smoothnessId,smoothness);
            block.SetFloat(clipId,clip);
            GetComponent<Renderer>().SetPropertyBlock(block);
        }
    }
}
