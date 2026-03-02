using UnityEngine;

public class SetActiveLevelDataOnStart : MonoBehaviour
{
    [SerializeField]
    private LevelData m_levels;

    void Start()
    {
        if(!string.IsNullOrEmpty(m_levels.m_sceneName))
        {
            LevelManager.SetCurrentLevel(m_levels);
            Destroy(this.gameObject);
        }
    }

}
