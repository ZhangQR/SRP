using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NiuBiSRP
{
    public class MeshBall : MonoBehaviour
    {
        static int baseColorID = Shader.PropertyToID("_BaseColor");
        static int metallicId = Shader.PropertyToID("_Metallic");
        static int smoothnessId = Shader.PropertyToID("_Smoothness");
        static int clipId = Shader.PropertyToID("_Clip");
        [SerializeField] private Mesh mesh;
        [SerializeField] private Material material;

        private Matrix4x4[] matrices = new Matrix4x4[1023];
        private Vector4[] colorList = new Vector4[1023];

        private float[] metallicList = new float[1023],
            smoothnessList = new float[1023],
            clipList = new float[1023];
        // private float[] smoothnessList = new float[1023];
        // private float[] clipList = new float[1023];
        private static MaterialPropertyBlock block;

        private void Awake()
        {
            if (block== null)
            {
                block = new MaterialPropertyBlock();
            }
            int length = matrices.Length;
            for (int i = 0; i < length; i++)
            {
                matrices[i] = Matrix4x4.TRS(
                    Random.insideUnitSphere * 10,
                    Quaternion.Euler(Random.value*360,Random.value*360,Random.value*360),
                    Random.Range(0.5f,1.3f)* Vector3.one);
                
                colorList[i] = new Vector4(Random.value, Random.value, Random.value, 1.0f);
                colorList[i] *= 0.5f;
                colorList[i] += Vector4.one * 0.5f;
                colorList[i].w = 1.0f;
                // metallicList[i] = Random.value < 0.25f ? 0.0f : 1.0f;
                metallicList[i] = Random.value;
                smoothnessList[i] = Random.Range(0.05f,0.95f);
                clipList[i] = Random.value;
            }
        }

        private void Update()
        {
            block.SetVectorArray(baseColorID,colorList);
            block.SetFloatArray(metallicId,metallicList);
            block.SetFloatArray(smoothnessId,smoothnessList);
            block.SetFloatArray(clipId,clipList);
            Graphics.DrawMeshInstanced(mesh,0,material,matrices,1023,block);
        }
    }
}
