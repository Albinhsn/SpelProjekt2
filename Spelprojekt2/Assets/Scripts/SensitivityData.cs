using UnityEngine;

[CreateAssetMenu(fileName = "SensitivityData", menuName = "ScriptableObjects/SensitivityData")]
public class SensitivityData : ScriptableObject
{
    public float m_currentSensitivity;
    public float m_maxSensitivity;
    public float m_minSensitivity;
}
