using UnityEngine;

namespace srUtils.Unity
{
    public class MBSceneManager : MonoBehaviour
    {
        public void OpenScene(int index)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(index);
        }
    }
}