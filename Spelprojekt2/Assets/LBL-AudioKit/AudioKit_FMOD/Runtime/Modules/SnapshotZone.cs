using UnityEngine;
using FMODUnity;
using FMOD.Studio;

using STOP_MODE = global::FMOD.Studio.STOP_MODE;

// AudioKit anteckning
// Trigger-zon som startar/stoppar en snapshot.
// Snapshot kan anges som:
// - EventReference (rekommenderas)
// - EventRegistry-id (via AudioResources.EventRegistry)
// - Rå FMOD path ("event:/Snapshots/...")

namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("AudioKit/Zones/Snapshot Zone")]
    public sealed class SnapshotZone : MonoBehaviour
    {
        [Header("Snapshot")]
        [Tooltip("Om satt används EventReference. Annars används snapshotIdOrPath.")]
        [SerializeField] private EventReference snapshotEvent;

        [Tooltip("EventRegistry-id eller FMOD path. Ignoreras om snapshotEvent är satt.")]
        [SerializeField] private string snapshotIdOrPath = "";

        [Tooltip("Om > 0 används ALLOWFADEOUT vid stop, annars IMMEDIATE.")]
        [SerializeField] private float fadeOutSeconds = 0.25f;

        [Header("Filter")]
        [SerializeField] private string requiredTag = "Player";
        [SerializeField] private LayerMask requiredLayers = ~0;

        private EventInstance inst;
        private int insideCount;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnDisable()
        {
            insideCount = 0;
            StopSnapshot(true);
        }

        private void OnDestroy()
        {
            StopAndRelease();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!PassesFilter(other)) return;

            insideCount++;
            if (insideCount == 1)
                StartSnapshot();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!PassesFilter(other)) return;

            insideCount = Mathf.Max(0, insideCount - 1);
            if (insideCount == 0)
                StopSnapshot(false);
        }

        private bool PassesFilter(Collider other)
        {
            if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag))
                return false;

            if (((1 << other.gameObject.layer) & requiredLayers.value) == 0)
                return false;

            return true;
        }

        private void EnsureInstance()
        {
            if (inst.isValid()) return;

            if (!snapshotEvent.IsNull)
            {
                inst = RuntimeManager.CreateInstance(snapshotEvent);
                return;
            }

            if (string.IsNullOrEmpty(snapshotIdOrPath)) return;

            var key = snapshotIdOrPath.Trim();
            var reg = AudioResources.EventRegistry;
            if (reg != null && reg.TryGetEvent(key, out var ev) && !ev.IsNull)
            {
                inst = RuntimeManager.CreateInstance(ev);
                return;
            }

            inst = RuntimeManager.CreateInstance(key);
        }

        private void StartSnapshot()
        {
            EnsureInstance();
            if (!inst.isValid()) return;
            inst.start();
        }

        private void StopSnapshot(bool immediate)
        {
            if (!inst.isValid()) return;
            var allow = !immediate && fadeOutSeconds > 0.001f;
            inst.stop(allow ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
        }

        private void StopAndRelease()
        {
            if (!inst.isValid()) return;
            inst.stop(STOP_MODE.ALLOWFADEOUT);
            inst.release();
            inst.clearHandle();
        }
    }
}
