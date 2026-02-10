using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.SceneManagement;

public class SceneLoader
{
    private LevelData m_level;
    private int m_scenesRemaining;


    public SceneLoader(LevelData level)
    {
        this.m_level = level;
        m_scenesRemaining =  1 + level.m_scene.m_subscenes.Length;

    }

    public bool m_allScenesAreLoaded => m_scenesRemaining == 0;

    private void OnSceneLoad(AsyncOperation handle)
    {
        --m_scenesRemaining;
    }

    public void Load()
    {
        var handle = SceneManager.LoadSceneAsync(m_level.m_sceneName, LoadSceneMode.Additive);
        handle.completed += OnSceneLoad;
        foreach(Subscene subscene in m_level.m_scene.m_subscenes)
        {
            handle = SceneManager.LoadSceneAsync(subscene.m_scene, LoadSceneMode.Additive);
            handle.completed += OnSceneLoad;
        }
    }
}
