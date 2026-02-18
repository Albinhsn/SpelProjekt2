using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[CreateAssetMenu(fileName = "SceneSkySettingsData", menuName = "ScriptableObjects/SceneSkySettings")]
public class SkySettingsData : ScriptableObject
{
    public float m_directionalLightEmissionInLux;
    public HDAdditionalCameraData.ClearColorMode m_clearKind;

    [Header("Only used if clear kind is Solid Color")]
    public Color m_clearColor;
}
