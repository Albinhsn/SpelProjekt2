using UnityEngine;

using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

/*

   Make a script in which you can assign each scene that exists and its offset
   Make some sort of trigger that can be acivated which loads in a scene
   Create a dummy plane in the main scene which has like 3 areas you can go to
   Start streaming in those scenes if you trigger something

*/


public class SceneStreamer : MonoBehaviour
{
    public AssetReference scene_reference;

    private AsyncOperationHandle<SceneInstance> m_loadHandle;

    void OnSceneLoad(AsyncOperationHandle<SceneInstance> scene_handle)
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
                root.transform.position += new Vector3(25, 0, 25);
            }
        }
    }

    float time_until_load = 2.0f;
    bool loaded;

    void Update()
    {
        time_until_load -= Time.deltaTime;
        if(time_until_load < 0 && !loaded)
        {
            loaded = true;
            m_loadHandle = Addressables.LoadSceneAsync(scene_reference, LoadSceneMode.Additive);
            m_loadHandle.Completed += OnSceneLoad;

        }
    }

}
