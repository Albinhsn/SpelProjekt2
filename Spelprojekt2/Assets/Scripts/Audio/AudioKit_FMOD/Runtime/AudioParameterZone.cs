using UnityEngine;

// AudioKit anteckning
// Zon som sätter global parameter
// Prio och fade per zon
// Valfritt parameternamn


namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class AudioParameterZone : MonoBehaviour
    {
        public enum ParameterMode { Known, Custom, Asset }

        [Header("Parameter")]
        [SerializeField] private ParameterMode mode = ParameterMode.Asset;

        // Behåller detta fält för att inte tappa gamla scene-refs
        [SerializeField] private KnownGlobalParam param = KnownGlobalParam.Indoor;

        // Nytt för återanvändning mellan spel
        [SerializeField] private string customParamName = "Indoor";

        [Tooltip("Rekommenderat: använd en AudioParameterSO istället för att skriva namn.")] 
        [SerializeField] private AudioParameterSO parameter;

        [Header("Värde")]
        [SerializeField] private float value = 1f;

        [Header("Prioritet")]
        [SerializeField] private int priority;
        [SerializeField] private float fadeSeconds = 0.15f;

        [Header("Target")]
        [SerializeField] private string requiredTag = "Player";
        [SerializeField] private bool resetOnDisable = true;
        [SerializeField] private bool forceTrigger = true;

        private int sourceId;
        private Collider zoneCollider;

        private void Reset()
        {
            zoneCollider = GetComponent<Collider>();
            if (zoneCollider != null) zoneCollider.isTrigger = true;
        }

        private void OnValidate()
        {
            if (!forceTrigger) return;
            var c = GetComponent<Collider>();
            if (c != null) c.isTrigger = true;
        }

        private void Awake()
        {
            sourceId = GetInstanceID();
            zoneCollider = GetComponent<Collider>();

            if (forceTrigger && zoneCollider != null)
                zoneCollider.isTrigger = true;
        }

        private void OnDisable()
        {
            if (!resetOnDisable) return;
            if (AudioParameterDriver.I == null) return;

            var name = ResolveName();
            if (string.IsNullOrEmpty(name)) return;

            AudioParameterDriver.I.SetSourceActive(name, sourceId, false, value, priority, fadeSeconds);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (string.IsNullOrEmpty(requiredTag)) return;
            if (!AudioActivatorUtil.HasTag(other, requiredTag)) return;
            Apply(true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (string.IsNullOrEmpty(requiredTag)) return;
            if (!AudioActivatorUtil.HasTag(other, requiredTag)) return;
            Apply(false);
        }

        private void Apply(bool active)
        {
            if (AudioParameterDriver.I == null) return;

            var name = ResolveName();
            if (string.IsNullOrEmpty(name)) return;

            AudioParameterDriver.I.SetSourceActive(name, sourceId, active, value, priority, fadeSeconds);
        }

        private string ResolveName()
        {
            if (mode == ParameterMode.Asset) return (parameter != null && parameter.IsValid) ? parameter.fmodName : null;
            if (mode == ParameterMode.Custom) return customParamName;
            if (AudioParameterDriver.I == null) return null;
            return AudioParameterDriver.I.Resolve(param);
        }
    }
}
