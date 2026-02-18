using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GlitchRespawnOnTrigger : MonoBehaviour
{

    void RespawnPlayer()
    {
        LevelCheckpointManager.Respawn();
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if(GlitchTransitionManager.StartTransition(2.0f, 0.0f))
            {
                GlitchTransitionManager.m_onTransitionEnd.AddListener(RespawnPlayer);
            }
        }

    }
}
