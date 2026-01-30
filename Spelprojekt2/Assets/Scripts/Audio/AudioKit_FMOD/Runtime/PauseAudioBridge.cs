using UnityEngine;

// AudioKit anteckning
// Bridge för pause snapshot
// Koppla från UI eller pause-system
// Påverkar mixen i FMOD


namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    public sealed class PauseAudioBridge : MonoBehaviour
    {
        public void SetPaused(bool paused)
        {
            if (AudioSystem.I != null)
                AudioSystem.I.SetPauseSnapshot(paused);
        }
    }
}
