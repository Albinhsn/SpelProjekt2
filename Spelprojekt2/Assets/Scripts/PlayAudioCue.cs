using UnityEngine;
using AudioKit.FMOD;

public class PlayAudioCue : MonoBehaviour
{
    [SerializeField]
    private AudioCueSO m_audioCue;


    public void Play()
    {
        SfxDirector.PlayCue2(m_audioCue, this.transform.position);

    }
}
