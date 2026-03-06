using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using Unity.Cinemachine;

public class SkySettings : MonoBehaviour
{
    [SerializeField]
    private SkySettingsData m_data;

    void Start()
    {
#if UNITY_EDITOR
        Apply();
#endif
    }
    public static void Apply(SkySettings settings)
    {
        SkySettingsData data = settings.m_data;
        if(data != null)
        {
            Camera cam = Camera.main;
            if(cam != null)
            {
                HDAdditionalCameraData camera_data = cam.gameObject.GetComponent<HDAdditionalCameraData>();
                camera_data.clearColorMode     = data.m_clearKind;
                camera_data.backgroundColorHDR = data.m_clearColor;

                Light sun       = RenderSettings.sun;
                sun.intensity   = data.m_directionalLightEmissionInLux;
            }

            PickupItemIndicatorManager.SetEmission(data.m_heldEmission, data.m_closestEmission, data.m_indicatorEmission);
        }
    }

    public  void Apply()
    {
        if(m_data != null)
        {
            Camera cam = Camera.main;
            if(cam != null)
            {
                HDAdditionalCameraData camera_data = cam.gameObject.GetComponent<HDAdditionalCameraData>();
                camera_data.clearColorMode     = m_data.m_clearKind;
                camera_data.backgroundColorHDR = m_data.m_clearColor;

                Light sun       = RenderSettings.sun;
                sun.intensity   = m_data.m_directionalLightEmissionInLux;

            }
            PickupItemIndicatorManager.SetEmission(m_data.m_heldEmission, m_data.m_closestEmission, m_data.m_indicatorEmission);
        }
    }
}
