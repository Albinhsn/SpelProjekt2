using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace SP2.Audio
{
    // Loop/ambience emitter with parameter support.
    // Use this when you need to drive event-local params.

    [DisallowMultipleComponent]
    public sealed class AudioEmitterPlus : MonoBehaviour
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

        public bool IsPlaying => _playing;

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
            Stop(immediate: true);
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
            Stop(immediate: false);
        }

        public void Stop(bool immediate)
        {
            if (!_playing) return;
            if (!_inst.isValid())
            {
                _playing = false;
                return;
            }

            STOP_MODE mode;

            if (immediate)
                mode = STOP_MODE.IMMEDIATE;
            else
                mode = allowFadeOutOnStop ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE;

            _inst.stop(mode);
            _inst.release();
            _inst.clearHandle();

            _playing = false;
        }

        // ---------------------------------
        //== Params ==
        // ---------------------------------
        public void SetParam(string paramName, float value)
        {
            if (!_inst.isValid()) return;
            if (string.IsNullOrEmpty(paramName)) return;
            _inst.setParameterByName(paramName, value);
        }

        public void SetParam01(string paramName, float value01)
        {
            SetParam(paramName, Mathf.Clamp01(value01));
        }

        public void Toggle(bool on)
        {
            if (on) Play();
            else Stop();
        }
    }
}
