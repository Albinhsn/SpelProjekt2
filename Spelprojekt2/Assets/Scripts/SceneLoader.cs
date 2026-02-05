using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class SceneLoader
{
    string m_sceneName;
    Vector3 m_offset;
    Player m_playerPrefab;

    public SceneLoader(string scene_name, Vector3 offset, Player player_prefab_to_load)
    {
        this.m_sceneName      = scene_name;
        this.m_playerPrefab   = player_prefab_to_load;
        this.m_offset         = offset;
    }

    private void OnSceneLoad(AsyncOperation handle)
    {
        Scene scene = SceneManager.GetSceneByName(m_sceneName);
        foreach(var root in scene.GetRootGameObjects())
        {
            if(root.name != "UniqueSceneRoot")
            {
                GameObject.Destroy(root);
            }
            else
            {
                root.transform.position += this.m_offset;
                Debug.Log($"Streaming in {scene.name} with index {scene.buildIndex}");
                SerializableObject[] serialized_objects = root.GetComponentsInChildren<SerializableObject>(true);
                if(serialized_objects != null && serialized_objects.Length != 0)
                {
                    PersistentDataManager.DeserializeScene(scene.name, serialized_objects);
                }
                else
                {
                    Debug.Log($"Found no serializable objects for {scene.name}");
                }
            }
        }

        if(m_playerPrefab != null)
        {
            Player player = GameObject.Instantiate(m_playerPrefab);
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

    public void Load()
    {

        var handle = SceneManager.LoadSceneAsync(m_sceneName, LoadSceneMode.Additive);
        handle.completed += OnSceneLoad;
    }
}
