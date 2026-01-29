using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace SP2.Audio
{
    // Anvand en snapshot-event i FMOD och trigga kort "duck"/"hit".
    //
    // Kalla Trigger() fran gameplay.
    public sealed class SnapshotPulse : MonoBehaviour
    {
        [SerializeField] private EventReference snapshotEvent;
        [Min(0f)] [SerializeField] private float holdSeconds = 0.15f;
        [SerializeField] private bool allowFadeOut = true;
        [Min(0f)] [SerializeField] private float minIntervalSeconds = 0.05f;
        [SerializeField] private bool useUnscaledTime = true;

        private EventInstance _inst;
        private float _stopAt;
        private float _lastTrigger;
        private bool _active;

        public void Trigger()
        {
            Trigger(holdSeconds);
        }

        public void Trigger(float holdOverride)
        {
            float now = useUnscaledTime ? Time.unscaledTime : Time.time;
            if (minIntervalSeconds > 0f && now - _lastTrigger < minIntervalSeconds)
                return;

            _lastTrigger = now;

            if (snapshotEvent.IsNull) return;
            if (!FmodSafe.EventExists(snapshotEvent)) return;

            if (!_inst.isValid())
                _inst = RuntimeManager.CreateInstance(snapshotEvent);

            if (!_active)
            {
                _inst.start();
                _active = true;
            }

            float h = Mathf.Max(0f, holdOverride);
            _stopAt = now + h;
        }

        private void Update()
        {
            if (!_active) return;

            float now = useUnscaledTime ? Time.unscaledTime : Time.time;
            if (now < _stopAt) return;

            StopInternal();
        }

        private void StopInternal()
        {
            if (!_inst.isValid())
            {
                _active = false;
                return;
            }

            _inst.stop(allowFadeOut ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
            _active = false;
        }

        private void OnDestroy()
        {
            if (_inst.isValid())
            {
                _inst.stop(STOP_MODE.IMMEDIATE);
                _inst.release();
                _inst.clearHandle();
            }
        }
    }
}
