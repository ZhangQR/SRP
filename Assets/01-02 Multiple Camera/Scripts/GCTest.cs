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
        // �������Ļ���Ӣ�ģ�һ���ַ����� 2 �ֽ�
        Debug.Log("��".ToCharArray().Length * sizeof(char));
    }

    // Update is called once per frame
    void Update()
    {
    }
}
