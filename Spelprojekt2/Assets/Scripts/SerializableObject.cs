using UnityEngine;
using UnityEditor;
using System;

public class SerializableObject : MonoBehaviour
{

    [SerializeField]
    public byte[] m_ID;

#if UNITY_EDITOR
    void OnValidate()
    {
        if(Application.isPlaying)
        {
            var id = GlobalObjectId.GetGlobalObjectIdSlow(this.gameObject);
            if(m_ID != null)
            {
                Debug.Log($"Validating {id} that has {new Guid(m_ID)}");
            }
            m_ID = GuidManager.Register(m_ID, this.gameObject);
        }
    }
#endif
}
