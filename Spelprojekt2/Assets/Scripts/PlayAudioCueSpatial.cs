using UnityEngine;
using AudioKit.FMOD;
using FMOD;
using FMODUnity;
using STOP_MODE = global::FMOD.Studio.STOP_MODE;

public class PlayAudioCueSpatial : MonoBehaviour
{
    [SerializeField]
    private AudioCueSO m_cue;

    [SerializeField]
    private bool m_fadeOut;

    private FMOD.Studio.EventInstance m_soundInstance;

    
    public void Play()
    {
        m_soundInstance = RuntimeManager.CreateInstance(m_cue.evt);
        m_soundInstance.set3DAttributes(RuntimeUtils.To3DAttributes(this.transform.position));
        m_soundInstance.start();
    }

    public void Stop()
    {
        m_soundInstance.stop(m_fadeOut ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
        m_soundInstance.release();
    }
}
