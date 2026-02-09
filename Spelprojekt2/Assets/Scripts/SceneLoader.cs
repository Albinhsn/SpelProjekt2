using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class SceneLoader
{
    SceneData m_scene;
    Player m_playerPrefab;
    int m_scenesRemaining;

    public SceneLoader(SceneData scene, Player player_prefab_to_load)
    {
        this.m_scene        = scene;
        this.m_playerPrefab = player_prefab_to_load;
    }

    private void OnSceneLoad(AsyncOperation handle)
    {
        m_scenesRemaining--;
        if(m_scenesRemaining == 0)
        {
            if(m_playerPrefab != null)
            {
                Player player            = GameObject.Instantiate(m_playerPrefab);
                FilterKind active_filter = PersistentDataManager.DeserializePlayer(player);
                if(active_filter != FilterKind.None)
                {
                    FilterManager fm = UnityEngine.Object.FindFirstObjectByType<FilterManager>();
                    if(fm != null)
                    {
                        fm.ChangeFilter(active_filter);
                    }
                    else
                    {
                        Debug.LogWarning("Tried to deserialize a player and change filter but found no filter manager");
                    }
                }
            }
        }
    }
    
    public void Load()
    {
        foreach(Subscene subscene in this.m_scene.m_subscenes)
        {
            var handle = SceneManager.LoadSceneAsync(subscene.m_scene, LoadSceneMode.Additive);
            handle.completed += OnSceneLoad;
        }
    }

    public void LoadBackground()
    {
        foreach(Subscene subscene in this.m_scene.m_subscenes)
        {
            var handle = SceneManager.LoadSceneAsync(subscene.m_scene, LoadSceneMode.Additive);
            handle.completed += OnSceneLoad;
        }
    }

    public void LoadTransition()
    {
        foreach(Subscene subscene in this.m_scene.m_subscenes)
        {
            var handle = SceneManager.LoadSceneAsync(subscene.m_scene, LoadSceneMode.Additive);
            handle.completed += OnSceneLoad;
        }
    }
}
