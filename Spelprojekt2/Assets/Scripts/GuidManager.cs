using UnityEngine;
using System.Collections.Generic;
using System;

public class GuidManager 
{
    private static GuidManager I;
    private Dictionary<Guid, GameObject> m_objs;

    public GuidManager()
    {
        m_objs = new();
    }

    public static bool Register(SerializableObject obj)
    {
        if(I == null)
        {
            I = new();
        }

        Guid id = obj.Guid;

        bool found  = !I.m_objs.ContainsKey(id);
        bool result = found;
        if(found)
        {
            I.m_objs[id] = obj.gameObject;
        }

        if(!found)
        {
            GameObject go = I.m_objs[id];
            if(go != null && obj.gameObject != go)
            {
                result = false;
            }
            else
            {
                result = true;
                I.m_objs[id] = obj.gameObject;
            }
        }

        return result;

    }
}
