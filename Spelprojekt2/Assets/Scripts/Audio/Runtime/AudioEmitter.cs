using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace SP2.Audio
{
    // Bra för rumston, maskinljud, maskiner, osv.
    [DisallowMultipleComponent]
    public sealed class AudioEmitter : MonoBehaviour
    {
        [Header("== AudioCue ==")]
        [SerializeField] private AudioCueSO cue;

        [Header("== Play/Stop ==")]
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool stopOnDisable = true;
        [SerializeField] private bool allowFadeOutOnStop = true;

        [Header("== Attach (3D) ==")]
        [SerializeField] private bool attachToThisTransform = true;
        [SerializeField] private Rigidbody optionalRigidbody;

        private EventInstance _inst;
        private bool _playing;

        private void Awake()
        {
            if (cue == null) return;
            if (!cue.preloadSampleData) return;
            if (cue.eventReference.IsNull) return;

            RuntimeManager.StudioSystem.getEventByID(cue.eventReference.Guid, out EventDescription desc);
            if (desc.isValid())
                desc.loadSampleData();
        }

        private void OnEnable()
        {
            if (playOnEnable)
                Play();
        }

        private void OnDisable()
        {
            if (stopOnDisable)
                Stop();
        }

        private void OnDestroy()
        {
            Stop();
        }

        public void Play()
        {
            if (_playing) return;
            if (cue == null) return;
            if (cue.eventReference.IsNull) return;

            if (!FmodSafe.EventExists(cue.eventReference))
                return;

            _inst = RuntimeManager.CreateInstance(cue.eventReference);

            if (!cue.is2D && attachToThisTransform)
                RuntimeManager.AttachInstanceToGameObject(_inst, transform, optionalRigidbody);

            _inst.start();
            _playing = true;
        }

        public void Stop()
        {
            if (!_playing) return;
            if (!_inst.isValid())
            {
                _playing = false;
                return;
            }

            _inst.stop(allowFadeOutOnStop ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
            _inst.release();
            _inst.clearHandle();

            _playing = false;
        }

        public void Toggle(bool on)
        {
            if (on) Play();
            else Stop();
        }
    }
}
