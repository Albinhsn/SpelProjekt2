#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
static class EditorSceneAutoLoader
{
    static EditorSceneAutoLoader()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        RootScene[] roots = UnityEngine.Object.FindObjectsOfType<RootScene>();
        foreach(RootScene root in roots)
        {
            if(root.gameObject.scene.name == scene.name)
            {
                if(root.m_sceneData != null)
                {
                    Subscene[] subscenes = root.m_sceneData.m_subscenes;
                    for(int i = 0; i < subscenes.Length; i++)
                    {
                        Subscene subscene = subscenes[i];
                        bool valid = false;
                        if(subscene.m_scene != null)
                        {
                            valid = true;
                            // TODO(ah): Verify this is a valid scene?
                        }

                        if(valid)
                        {
                            string path = subscene.m_scenePath + subscene.m_scene + ".unity";
                            EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                        }
                    }

                }
            }
        }

        if(roots != null && scene.name != "MainSceneItems")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/MainSceneItems.unity", OpenSceneMode.Additive);
        }
    }
}
#endif
