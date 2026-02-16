using UnityEngine;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

public class SerializableObject : MonoBehaviour
{
    private Guid m_guid;
    public Guid m_ID => m_guid;

    private void Awake()
    {
        List<byte> data = new List<byte>(16);
            
        data.Add((byte) gameObject.scene.buildIndex);
        foreach (char obj in gameObject.name)
        {
            data.AddRange(BitConverter.GetBytes(obj));
        }
        
        data.AddRange(BitConverter.GetBytes(transform.localToWorldMatrix.m00));
        data.AddRange(BitConverter.GetBytes(transform.localToWorldMatrix.m01));
        data.AddRange(BitConverter.GetBytes(transform.localToWorldMatrix.m02));
        data.AddRange(BitConverter.GetBytes(transform.localToWorldMatrix.m03));
        data.AddRange(BitConverter.GetBytes(transform.localToWorldMatrix.m10));
        data.AddRange(BitConverter.GetBytes(transform.localToWorldMatrix.m11));
        data.AddRange(BitConverter.GetBytes(transform.localToWorldMatrix.m12));
        data.AddRange(BitConverter.GetBytes(transform.localToWorldMatrix.m13));
        data.AddRange(BitConverter.GetBytes(transform.localToWorldMatrix.m20));
        data.AddRange(BitConverter.GetBytes(transform.localToWorldMatrix.m21));
        data.AddRange(BitConverter.GetBytes(transform.localToWorldMatrix.m22));
        data.AddRange(BitConverter.GetBytes(transform.localToWorldMatrix.m23));
        data.AddRange(BitConverter.GetBytes(transform.localToWorldMatrix.m30));
        data.AddRange(BitConverter.GetBytes(transform.localToWorldMatrix.m31));
        data.AddRange(BitConverter.GetBytes(transform.localToWorldMatrix.m32));
        data.AddRange(BitConverter.GetBytes(transform.localToWorldMatrix.m33));
        

        using (MD5 md5 = MD5.Create())
        {
            m_guid = new Guid(md5.ComputeHash(data.ToArray()));
        }
        
        Debug.Log(m_guid.ToString());
    }
}
