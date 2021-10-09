using System.Collections;
using System.Collections.Generic;
using UnityEngine;
class A
{

}

public class GCTest : MonoBehaviour
{
    A a;
    // Start is called before the first frame update
    void Start()
    {
        //a = new A();
        // 不管中文还是英文，一个字符都是 2 字节
        Debug.Log("哈".ToCharArray().Length * sizeof(char));
    }

    // Update is called once per frame
    void Update()
    {
    }
}
