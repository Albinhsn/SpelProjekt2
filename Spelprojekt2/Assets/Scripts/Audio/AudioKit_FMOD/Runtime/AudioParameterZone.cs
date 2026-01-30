using UnityEngine;

// AudioKit anteckning
// Zon som sätter en global parameter
// Prio och fade per zon
// Kräver Player-tag


namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    public sealed class AudioParameterZone : MonoBehaviour
    {
        [SerializeField] private KnownGlobalParam param = KnownGlobalParam.Indoor;
        [SerializeField] private float value = 1f;
        [SerializeField] private int priority;
        [SerializeField] private float fadeSeconds = 0.15f;

        private int sourceId;

        private void Reset()
        {
            var c = GetComponent<Collider>();
            if (c != null) c.isTrigger = true;
        }

        private void Awake()
        {
            sourceId = GetInstanceID();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            Apply(true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            Apply(false);
        }

        private void Apply(bool active)
        {
            if (AudioParameterDriver.I == null) return;

            var name = AudioParameterDriver.I.Resolve(param);
            AudioParameterDriver.I.SetSourceActive(name, sourceId, active, value, priority, fadeSeconds);
        }
    }
}
