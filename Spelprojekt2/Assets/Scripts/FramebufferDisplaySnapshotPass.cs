using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

class FramebufferDisplaySnapshotPass : CustomPass
{

    public static bool m_activated;

    public static void Activate()
    {
        m_activated = true;
    }

    public static void Deactivate()
    {
        m_activated = false;
    }

    protected override void Execute(CustomPassContext ctx)
    {
        if(m_activated)
        {
            var I = FramebufferSnapshotManager.I;
            if(I != null)
            {
                HDUtils.BlitCameraTexture(ctx.cmd, I.m_handle, ctx.cameraColorBuffer);
            }
        }
    }

}
