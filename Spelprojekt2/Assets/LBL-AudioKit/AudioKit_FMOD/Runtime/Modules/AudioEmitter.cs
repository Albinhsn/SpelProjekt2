using UnityEngine;
using FMODUnity;
using FMOD.Studio;

using STOP_MODE = global::FMOD.Studio.STOP_MODE;

// Spelar ett event som följer ett objekt
// Bra för loops och ambience
// Start på enable, stop på disable

namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    public sealed class AudioEmitter : MonoBehaviour
    {
        [SerializeField] private AudioCueSO cue;
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool stopOnDisable = true;
        [SerializeField] private bool fadeOut = true;

        private EventInstance inst;
        private Rigidbody cachedRb;

        private void Awake()
        {
            cachedRb = GetComponentInParent<Rigidbody>();
        }

        private void OnEnable()
        {
            if (playOnEnable) Play();
        }

        private void OnDisable()
        {
            if (stopOnDisable) Stop();
        }

        private void OnDestroy()
        {
            Stop();
            // OBS: ladda IN sampledata vid behov, men undvik att unloada här (cue delas ofta mellan flera ljud).
        }

        public void Play()
        {
            if (cue == null) return;
            if (cue.evt.IsNull) return;

            cue.EnsureSampleDataLoaded();

            if (!inst.isValid())
            {
                inst = RuntimeManager.CreateInstance(cue.evt);

                // FIX: Rigidbody kan sitta på parent (t.ex. Player-root)
                if (cachedRb == null) cachedRb = GetComponentInParent<Rigidbody>();
                RuntimeManager.AttachInstanceToGameObject(inst, gameObject, cachedRb);
            }

            inst.start();
        }

        public void Stop()
        {
            if (!inst.isValid()) return;

            inst.stop(fadeOut ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
            inst.release();
            inst.clearHandle();
            inst = default;
        }

        public void SetParam(string paramName, float value)
        {
            if (!inst.isValid()) return;
            if (string.IsNullOrEmpty(paramName)) return;
            inst.setParameterByName(paramName, value);
        }
    }
}
