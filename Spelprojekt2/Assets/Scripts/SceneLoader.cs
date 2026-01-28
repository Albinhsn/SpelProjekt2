using UnityEngine;

using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class SceneLoader : MonoBehaviour
{
    [SerializeField]
    private AssetReference m_sceneReference;
    [SerializeField]
    private Vector3 m_offset;

    private AsyncOperationHandle<SceneInstance> m_loadHandle;

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
                root.transform.position += m_offset;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            m_loadHandle = Addressables.LoadSceneAsync(m_sceneReference, LoadSceneMode.Additive);
            m_loadHandle.Completed += OnSceneLoad;
        }
    }
}
