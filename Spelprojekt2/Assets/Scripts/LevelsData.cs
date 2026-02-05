using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct LevelData
{
    public AssetReference m_sceneReference;
    public Vector3 m_offset;
}

[CreateAssetMenu(fileName = "LevelsData", menuName = "ScriptableObjects/LevelsData")]
public class LevelsData : ScriptableObject
{
    public LevelData[] levels;
}
