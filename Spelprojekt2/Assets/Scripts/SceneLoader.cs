using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class SceneLoader
{

    AssetReference m_sceneReference;
    Vector3 m_offset;

    public SceneLoader(AssetReference scene_reference, Vector3 offset)
    {
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
            }
        }
    }

    public void Load()
    {
        var handle = Addressables.LoadSceneAsync(m_sceneReference, LoadSceneMode.Additive);
        handle.Completed += OnSceneLoad;
    }
}
