using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct LevelData
{
    public string m_sceneName;
    public string m_scenePath;
    public SceneData m_scene;
}

[CreateAssetMenu(fileName = "LevelsData", menuName = "ScriptableObjects/LevelsData")]
public class LevelsData : ScriptableObject
{
    public LevelData[] m_levels;
}
