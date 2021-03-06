using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Catlike0202
{
    public class MeshBall : MonoBehaviour
    {
        static int baseColorID = Shader.PropertyToID("_BaseColor");
        [SerializeField] private Mesh mesh;
        [SerializeField] private Material material;

        private Matrix4x4[] matrices = new Matrix4x4[1023];
        private Vector4[] colorList = new Vector4[1023];
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
                    Random.value * 0.2f * Vector3.one);

                colorList[i] = new Vector4(Random.value, Random.value, Random.value, 1);
            }
        }

        private void Update()
        {
            block.SetVectorArray(baseColorID,colorList);
            // 这个 count 不能超过 1023
            Graphics.DrawMeshInstanced(mesh,0,material,matrices,1023,block);
        }
    }
}
