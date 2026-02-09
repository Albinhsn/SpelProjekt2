using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct Subscene
{
    public string m_scene; 
    public string m_scenePath; 
}

[CreateAssetMenu(fileName = "SceneData", menuName = "ScriptableObjects/SceneData")]
public class SceneData : ScriptableObject
{
    public Subscene[] m_subscenes;
}
