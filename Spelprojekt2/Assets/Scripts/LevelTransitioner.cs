using UnityEngine;

public class LevelTransitioner : MonoBehaviour
{
    [SerializeField]
    private LevelData m_levelToTransitionTo;

    [SerializeField]
    private bool m_respawnPlayerOnTransition;

    void RespawnPlayerOnSceneLoad()
    {
        LevelCheckpointManager.Respawn();
        LevelManager.m_onTransitionEnd -= RespawnPlayerOnSceneLoad;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if(m_respawnPlayerOnTransition)
            {
                LevelManager.m_onTransitionEnd += RespawnPlayerOnSceneLoad;
            }
            LevelManager.TransitionToSceneAsync(m_levelToTransitionTo);
        }
    }
}

