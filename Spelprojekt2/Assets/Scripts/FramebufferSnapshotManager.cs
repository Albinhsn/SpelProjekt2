using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class FramebufferSnapshotManager : MonoBehaviour
{
    public static FramebufferSnapshotManager I;

    public RTHandle m_handle;
    public bool m_requested;

    void Start()
    {
        if(I != null && I != this)
        {
            return;
        }

        I = this;

    }

    public static void Request()
    {
        FramebufferSnapshotManager.I.m_requested = true;
    }

    void OnDestroy()
    {
        m_handle?.Release();
    }
}
