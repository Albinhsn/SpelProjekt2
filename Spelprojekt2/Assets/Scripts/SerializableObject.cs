using UnityEngine;
using System;

public class SerializableObject : MonoBehaviour
{
    public Guid m_ID;

#if UNITY_EDITOR
    public void OnValidate()
    {
        if(m_ID == null)
        {
            m_ID = System.Guid.NewGuid();
        }
    }
#endif
}
