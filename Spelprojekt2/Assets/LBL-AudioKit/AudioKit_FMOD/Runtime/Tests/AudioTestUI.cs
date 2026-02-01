using UnityEngine;

namespace AudioKit.FMOD
{
    /// <summary>
    /// Enkel UI-driver för AudioTest-scen.
    /// Inga hårdkodade FMOD-namn krävs: fyll i cues/param i Inspector.
    /// </summary>
    [AddComponentMenu("AudioKit/Test/Audio Test UI")]
    public sealed class AudioTestUI : MonoBehaviour
    {
        [Header("Cues")]
        public AudioCueSO oneShotCue;
        public AudioCueSO loopCue;

        [Header("Emitter (recommended)")]
        public AudioEmitter loopEmitter;

        [Header("EventRegistry (optional)")]
        [Tooltip("Valfritt: test av AudioEventHub registry-loop. Ange ID här (t.ex. TEST_LOOP).")]
        public string loopRegistryId = "";

        [Header("Test Param (global)")]
        public AudioParamRef testParam;
        [Range(0f, 1f)] public float paramOnValue = 1f;
        [Range(0f, 1f)] public float paramOffValue = 0f;

        public void PlayOneShot()
        {
            if (SfxDirector.I == null) return;
            if (oneShotCue == null) return;

            SfxDirector.I.PlayCue(oneShotCue, GetPos());
        }

        public void StartLoop()
        {
            if (loopEmitter != null)
            {
                loopEmitter.Play();
                return;
            }

            // Alternativ: EventRegistry-id via AudioEventHub
            if (AudioEventHub.I != null && !string.IsNullOrEmpty(loopRegistryId))
                AudioEventHub.I.StartLoop(loopRegistryId);
        }

        public void StopLoop()
        {
            if (loopEmitter != null)
            {
                loopEmitter.Stop();
                return;
            }

            if (AudioEventHub.I != null && !string.IsNullOrEmpty(loopRegistryId))
                AudioEventHub.I.Stop(loopRegistryId, true);
        }

        public void StartMusic() => MusicDirector.I?.StartMusic();
        public void StopMusic()  => MusicDirector.I?.StopMusic(true);

        public void SetParamOn()  => SetParam(paramOnValue);
        public void SetParamOff() => SetParam(paramOffValue);

        private void SetParam(float v)
        {
            var name = testParam.Resolve(AudioResources.ParamLibrary);
            name = AudioParamUtil.Resolve(name);
            if (string.IsNullOrEmpty(name)) return;

            AudioSystem.I?.SetGlobalParam(name, v);
        }

        private Vector3 GetPos()
        {
            var cam = Camera.main;
            return cam != null ? cam.transform.position : Vector3.zero;
        }
    }
}
