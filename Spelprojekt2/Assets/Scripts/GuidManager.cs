using UnityEngine;
using System.Collections.Generic;
using System;

public class GuidManager 
{
    private static GuidManager I;
    private Dictionary<Guid, GameObject> m_objs;
    private Dictionary<GameObject, Guid> m_ids;

    public GuidManager()
    {
        m_ids  = new();
        m_objs = new();
    }

    public static byte[] Register(byte[] id, GameObject obj)
    {
        if(I == null)
        {
            I = new();
        }

        byte[] result = null;

        if(I.m_ids.ContainsKey(obj))
        {
            result = I.m_ids[obj].ToByteArray();
            Debug.Log($"Already found {obj.name} with {I.m_ids[obj]}");
        }
        else
        {
            for(;result == null;)
            {
                Guid guid = System.Guid.NewGuid();
                if(!I.m_objs.ContainsKey(guid))
                {
                    I.m_objs[guid] = obj;
                    I.m_ids[obj] = guid;
                    result = guid.ToByteArray();
                    Debug.Log($"Registered {obj.name} with {I.m_ids[obj]}");
                }
            }
        }

        return result;

    }
}
