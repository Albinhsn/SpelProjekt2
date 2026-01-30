using UnityEngine;
using FMODUnity;
using FMOD.Studio;

using STOP_MODE = global::FMOD.Studio.STOP_MODE;
// AudioKit anteckning
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

        private void Reset()
        {
            var c = GetComponent<Collider>();
            if (c != null) c.isTrigger = true;
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
            if (cue != null) cue.EnsureSampleDataUnloaded();
        }

        public void Play()
        {
            if (cue == null) return;
            if (cue.evt.IsNull) return;
            if (inst.isValid()) return;

            inst = RuntimeManager.CreateInstance(cue.evt);
            RuntimeManager.AttachInstanceToGameObject(inst, transform, GetComponent<Rigidbody>());
            inst.start();
        }

        public void Stop()
        {
            if (!inst.isValid()) return;
            inst.stop(fadeOut ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
            inst.release();
            inst.clearHandle();
        }

        public void SetParam(string paramName, float value)
        {
            if (!inst.isValid()) return;
            if (string.IsNullOrEmpty(paramName)) return;
            inst.setParameterByName(paramName, value);
        }
    }
}
