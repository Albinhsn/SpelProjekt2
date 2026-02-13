using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

[Serializable, VolumeComponentMenu("Post-processing/Custom/GlitchVolume")]
public sealed class GlitchVolume : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Header("Scanline settings")]
    public ClampedFloatParameter m_scanlineSpeed = new ClampedFloatParameter(0f, 0f, 1f);
    public ClampedFloatParameter m_scanlineStrength= new ClampedFloatParameter(0f, 0f, 1f);

    [Header("Glitch settings")]
    public ClampedFloatParameter m_glitchSpeed    = new ClampedFloatParameter(0f, 0f, 100f);
    public ClampedFloatParameter m_glitchStrength = new ClampedFloatParameter(0f, 0f, 100f);

    Material m_Material;

    [Header("Animated settings used at runtime because Unity is a piece of shit")]
    public ClampedFloatParameter m_intensity = new ClampedFloatParameter(0f, 0f, 1f);

    public bool IsActive() => m_Material != null;
    public bool IsActive2() => m_Material != null && m_intensity.value > 0;

    // Do not forget to add this post process in the Custom Post Process Orders list (Project Settings > Graphics > HDRP Global Settings).
    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    const string kShaderName = "Shader/GlitchVolume";

    public override void Setup()
    {
        if (Shader.Find(kShaderName) != null)
            m_Material = new Material(Shader.Find(kShaderName));
        else
            Debug.LogError($"Unable to find shader '{kShaderName}'. Post Process Volume GlitchVolume is unable to load. To fix this, please edit the 'kShaderName' constant in GlitchVolume.cs or change the name of your custom post process shader.");
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (!IsActive2())
            return;

        m_Material.SetFloat("_ScanlineSpeed", m_scanlineSpeed.value * m_intensity.value);
        m_Material.SetFloat("_ScanlineStrength", m_scanlineStrength.value * m_intensity.value);
        m_Material.SetFloat("_GlitchSpeed", m_glitchSpeed.value * m_intensity.value);
        m_Material.SetFloat("_GlitchStrength", m_glitchStrength.value * m_intensity.value);
        m_Material.SetTexture("_MainTex", source);
        HDUtils.DrawFullScreen(cmd, m_Material, destination, shaderPassId: 0);
    }

    public override void Cleanup()
    {
        CoreUtils.Destroy(m_Material);
    }
}
