using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class SceneLoader
{

    AssetReference m_sceneReference;
    Vector3 m_offset;
    Player m_playerPrefab;

    public SceneLoader(AssetReference scene_reference, Vector3 offset, Player player_prefab_to_load)
    {
        this.m_playerPrefab   = player_prefab_to_load;
        this.m_sceneReference = scene_reference;
        this.m_offset         = offset;
    }

    private void OnSceneLoad(AsyncOperationHandle<SceneInstance> scene_handle)
    {
        Scene scene = scene_handle.Result.Scene;
        foreach(var root in scene.GetRootGameObjects())
        {
            if(root.name != "UniqueSceneRoot")
            {
                GameObject.Destroy(root);
            }
            else
            {
                root.transform.position += this.m_offset;
                Debug.Log($"Streaming in {scene.name}");
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
                FilterManager fm = Object.FindFirstObjectByType<FilterManager>();
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

        var handle = Addressables.LoadSceneAsync(m_sceneReference, LoadSceneMode.Additive);
        handle.Completed += OnSceneLoad;
    }
}
