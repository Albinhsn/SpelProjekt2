using UnityEngine;

public class LevelTransitioner : MonoBehaviour
{
    [SerializeField]
    private LevelData m_levelToTransitionTo;

    [SerializeField]
    private float m_transitionTime = 4.0f;

    private bool m_transitioned;

    public void Transition()
    {
        if(!m_transitioned)
        {
            LevelManager.TransitionToSceneAsync(m_levelToTransitionTo, m_transitionTime);
            m_transitioned = true;
        }

    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            Transition();
        }
    }
}

