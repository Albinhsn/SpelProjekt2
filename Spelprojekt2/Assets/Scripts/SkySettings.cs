using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using Unity.Cinemachine;

public class SkySettings : MonoBehaviour
{
    [SerializeField]
    private SkySettingsData m_data;

    public void Apply()
    {
        if(m_data != null)
        {
            Camera cam = Camera.main;
            HDAdditionalCameraData camera_data = cam.gameObject.GetComponent<HDAdditionalCameraData>();
            camera_data.clearColorMode     = m_data.m_clearKind;
            camera_data.backgroundColorHDR = m_data.m_clearColor;

            Light sun       = RenderSettings.sun;
            sun.intensity   = m_data.m_directionalLightEmissionInLux;
        }
    }
}
