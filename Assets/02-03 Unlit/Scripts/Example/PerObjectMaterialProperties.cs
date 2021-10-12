using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
// Random.value 是 UnityEngine 里面的
using Random = UnityEngine.Random;

namespace Catlike0203
{
    public class PerObjectMaterialProperties : MonoBehaviour
    {
        [SerializeField] Color baseColor;
        static int baseColorId = Shader.PropertyToID("_BaseColor");
        
        static MaterialPropertyBlock block;
        
        private void Awake()
        {
            baseColor = new Color(Random.value, Random.value, Random.value);
            OnValidate();
        }
        
        void OnValidate()
        {
            if (block == null)
            {
                block = new MaterialPropertyBlock();
            }
            block.SetColor(baseColorId,baseColor);
            GetComponent<Renderer>().SetPropertyBlock(block);
        }
    }
}
