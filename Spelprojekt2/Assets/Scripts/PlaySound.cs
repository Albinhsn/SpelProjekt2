using UnityEngine;
using AudioKit.FMOD;
using Interaction;

public class PlaySound : MonoBehaviour
{
    public AudioCueSO m_soundCue;

    public void Play()
    {
        SfxDirector.PlayCue2(m_soundCue, this.transform.position);
    }
}
