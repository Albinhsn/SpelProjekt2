using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct LevelData
{
    public string m_sceneName;
    public Vector3 m_offset;
}

[CreateAssetMenu(fileName = "LevelsData", menuName = "ScriptableObjects/LevelsData")]
public class LevelsData : ScriptableObject
{
    public LevelData[] levels;
}
