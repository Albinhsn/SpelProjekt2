using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

class FramebufferSnapshotPass : CustomPass
{
    protected override void Setup(ScriptableRenderContext ctx, CommandBuffer cmd)
    {
        var I = FramebufferSnapshotManager.I;

        if(I != null)
        {
            I.m_handle = RTHandles.Alloc(
                Vector2.one,
                TextureXR.slices,
                dimension: TextureXR.dimension,
                colorFormat: GraphicsFormat.R16G16B16A16_SFloat,
                useDynamicScale: false,
                name: "Snapshot"
            );
        }

    }

    protected override void Execute(CustomPassContext ctx)
    {
        var cam = ctx.hdCamera.camera;
        if(cam == Camera.main)
        {
            var I = FramebufferSnapshotManager.I;
            if(I != null && I.m_requested)
            {
                I.m_requested = false;
                HDUtils.BlitCameraTexture(ctx.cmd, ctx.cameraColorBuffer,
                        I.m_handle);
            }
        }
    }
}
