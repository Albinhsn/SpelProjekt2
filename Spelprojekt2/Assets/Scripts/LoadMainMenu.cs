using UnityEngine;
using AudioKit.FMOD;
using UnityEngine.SceneManagement;

public class LoadMainMenu : MonoBehaviour
{
    public string m_scene;

    void FinishMainMenu(AsyncOperation op)
    {
        // ah: Change music
        AudioSceneSettings audio = UnityEngine.Object.FindFirstObjectByType<AudioSceneSettings>();
        if(audio != null)
        {
            audio.ApplyActions();
        }
        Destroy(this.gameObject);
    }

    void Start()
    {
        var handle = SceneManager.LoadSceneAsync(m_scene, LoadSceneMode.Additive);
        handle.completed += FinishMainMenu;

    }
}
