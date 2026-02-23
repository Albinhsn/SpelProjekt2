using UnityEngine;
using AudioKit.FMOD;

public class PlaySound : MonoBehaviour
{
    public AudioCueSO m_soundCue;


    public void Play()
    {
        SfxDirector.PlayCue2(m_soundCue, this.transform.position);
    }
}
