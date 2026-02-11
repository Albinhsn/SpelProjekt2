using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadMainMenu : MonoBehaviour
{
    public string m_scene;

    void Awake()
    {
        SceneManager.LoadSceneAsync(m_scene, LoadSceneMode.Additive);
        Destroy(this.gameObject);
    }
}
