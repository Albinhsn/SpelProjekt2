using UnityEngine;
using System;

public class SerializableObject : MonoBehaviour
{
    public string ID;

#if UNITY_EDITOR
    public void OnValidate()
    {
        if(ID == null)
        {
            ID = System.Guid.NewGuid().ToString();
        }
    }
#endif
}
