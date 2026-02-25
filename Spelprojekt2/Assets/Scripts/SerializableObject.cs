using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;
#endif

[ExecuteInEditMode, DisallowMultipleComponent]
public class SerializableObject : MonoBehaviour, ISerializationCallbackReceiver
{

    [SerializeField]
    public bool m_serializePositionAndRotation;

    private Guid guid = Guid.Empty;

    public Guid Guid => guid;

    [SerializeField]
    private byte[] m_ID;

    public byte[] m_id => m_ID;

    void CreateGuid()
    {

        if(m_ID == null || m_ID.Length != 16)
        {
#if UNITY_EDITOR
            if(IsAssetOnDisk())
            {
                return;
            }
            Undo.RecordObject(this, "Added GUID");
#endif

            bool is_null = m_ID == null;
            guid = Guid.NewGuid();
            m_ID = guid.ToByteArray();


#if UNITY_EDITOR
            if(PrefabUtility.IsPartOfNonAssetPrefabInstance(this))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(this);
            }
#endif
        }
        else if(guid == Guid.Empty)
        {
            guid = new Guid(m_ID);
        }


        if(guid != Guid.Empty)
        {

            if(!GuidManager.Register(this))
            {
                m_ID = null;
                guid = Guid.Empty;
                CreateGuid();
            }
            else
            {
                guid = new Guid(m_ID);
            }
        }

    }
#if UNITY_EDITOR
    private bool IsEditingInPrefabMode()
    {
        if (EditorUtility.IsPersistent(this))
        {
            return true;
        }
        else
        {
            var main = StageUtility.GetMainStageHandle();
            var current = StageUtility.GetStageHandle(this.gameObject);
            if (current != main)
            {
                var prefab = PrefabStageUtility.GetPrefabStage(gameObject);
                if (prefab != null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool IsAssetOnDisk()
    {
        return PrefabUtility.IsPartOfPrefabAsset(this) || IsEditingInPrefabMode();
    }


#endif

    public void OnBeforeSerialize()
    {
#if UNITY_EDITOR
        if (IsAssetOnDisk())
        {
            m_ID = null;
            guid = System.Guid.Empty;
        }
        else
#endif
        {
            if (guid != System.Guid.Empty)
            {
                m_ID = guid.ToByteArray();
            }
        }
    }

    public void OnAfterDeserialize()
    {
        if (m_ID != null && m_ID.Length == 16)
        {
            guid = new Guid(m_ID);
        }
    }

    void Awake()
    {
        CreateGuid();
    }

    void OnValidate()
    {
#if UNITY_EDITOR
        if (IsAssetOnDisk())
        {
            m_ID = null;
            guid = System.Guid.Empty;
        }
        else
#endif
        {
            CreateGuid();
        }
    }
}
