using UnityEngine;
using FMODUnity;
using FMOD.Studio;

using STOP_MODE = global::FMOD.Studio.STOP_MODE;
// AudioKit anteckning
// Kort snapshot-puls
// Bra för damage eller hit
// Kan även använda AudioSystem damage snapshot


namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    public sealed class SnapshotPulse : MonoBehaviour
    {
        [SerializeField] private EventReference snapshotEvent;
        [SerializeField] private float holdSeconds = 0.08f;

        private EventInstance inst;

        private void Awake()
        {
            if (!snapshotEvent.IsNull)
                inst = RuntimeManager.CreateInstance(snapshotEvent);
        }

        private void OnDestroy()
        {
            if (!inst.isValid()) return;
            inst.stop(STOP_MODE.ALLOWFADEOUT);
            inst.release();
            inst.clearHandle();
        }

        public void Pulse()
        {
            if (AudioSystem.I != null && snapshotEvent.IsNull)
            {
                AudioSystem.I.PulseDamageSnapshot(holdSeconds);
                return;
            }

            if (!inst.isValid()) return;
            inst.start();
            Invoke(nameof(StopSnapshot), holdSeconds);
        }

        private void StopSnapshot()
        {
            if (!inst.isValid()) return;
            inst.stop(STOP_MODE.ALLOWFADEOUT);
        }
    }
}
