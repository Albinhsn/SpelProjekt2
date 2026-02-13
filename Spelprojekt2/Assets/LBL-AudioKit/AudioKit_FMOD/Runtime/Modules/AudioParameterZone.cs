using UnityEngine;

// AudioKit anteckning
// Triggerzon som sätter en global parameter via AudioParameterDriver
// Bra för t.ex. Indoor/Outdoor, DangerZone, LowGravity, osv

namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("AudioKit/Zoner/Audio Parameter Zone")]
    public sealed class AudioParameterZone : MonoBehaviour
    {
        [Header("Parameter")]
        [SerializeField] private AudioParamRef parameter;

        [Header("Värden")]
        [Range(0f, 1f)] [SerializeField] private float valueInside = 1f;
        [Range(0f, 1f)] [SerializeField] private float valueOutside = 0f;
        [SerializeField] private float fadeSeconds = 0.25f;
        [SerializeField] private int priority = 0;
        [SerializeField] private string sourceId = "zone";

        [Header("Trigger")]
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private bool setOnEnter = true;
        [SerializeField] private bool setOnExit = true;

        // Legacy (gömda) - för att inte sabba gamla scener
        [SerializeField, HideInInspector] private ParameterMode legacyMode = ParameterMode.Custom;
        [SerializeField, HideInInspector] private KnownGlobalParam legacyKnown = KnownGlobalParam.Custom;
        [SerializeField, HideInInspector] private string legacyCustomParamName = "";
        [SerializeField, HideInInspector] private AudioParameterSO legacyParameter;

        private enum ParameterMode { Known, Custom, Asset }

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col) col.isTrigger = true;
        }

        private string ResolveParamName()
        {
            var resolved = parameter.Resolve(AudioResources.ParamLibrary);
            if (!string.IsNullOrEmpty(resolved)) return resolved;

            // fallback legacy
            if (legacyMode == ParameterMode.Asset && legacyParameter != null && legacyParameter.IsValid)
                return legacyParameter.fmodName;

            if (legacyMode == ParameterMode.Known)
            {
                // Legacy "Known" resolveas nu via AudioParamLibrary (key -> FMOD-namn)
                // för att slippa hårdkodade param-fält i AudioConfig.
                var name = AudioParameterDriver.I != null
                    ? AudioParameterDriver.I.Resolve(legacyKnown)
                    : null;

                if (!string.IsNullOrEmpty(name))
                    return name;
            }

            return AudioParamUtil.Resolve(legacyCustomParamName);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!setOnEnter) return;
            if (!other.CompareTag(playerTag)) return;

            var name = ResolveParamName();
            if (string.IsNullOrEmpty(name)) return;

            EnsureDriver();
            AudioParameterDriver.I.SetSourceActive(sourceId, name, valueInside, true, priority, fadeSeconds);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!setOnExit) return;
            if (!other.CompareTag(playerTag)) return;

            var name = ResolveParamName();
            if (string.IsNullOrEmpty(name)) return;

            EnsureDriver();
            AudioParameterDriver.I.SetSourceActive(sourceId, name, valueOutside, true, priority, fadeSeconds);
        }

        private static void EnsureDriver()
        {
            if (AudioParameterDriver.I != null) return;

            var go = new GameObject("AudioParameterDriver");
            go.AddComponent<AudioParameterDriver>();
        }
    }
}
