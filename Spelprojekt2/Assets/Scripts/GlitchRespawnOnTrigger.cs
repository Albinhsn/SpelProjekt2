using UnityEngine;
using AudioKit.FMOD;

[RequireComponent(typeof(Collider))]
public class GlitchRespawnOnTrigger : MonoBehaviour
{
    [SerializeField]
    private AudioCueSO m_onDeathAudioCue;

    void RespawnPlayer()
    {
        LevelCheckpointManager.Respawn();
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if(GlitchTransitionManager.StartTransition(2.5f, 0.0f))
            {
                GlitchTransitionManager.m_onTransitionEnd.AddListener(RespawnPlayer);
            }

            SfxDirector.PlayCue2(m_onDeathAudioCue, Vector3.zero);
        }

    }
}
