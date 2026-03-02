using UnityEngine;

public class LevelTransitioner : MonoBehaviour
{
    [SerializeField]
    private LevelData m_levelToTransitionTo;

    private bool m_transitioned;

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player" && !m_transitioned)
        {
            LevelManager.TransitionToSceneAsync(m_levelToTransitionTo);
            m_transitioned = true;
        }
    }
}

