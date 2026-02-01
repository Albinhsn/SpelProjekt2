using UnityEngine;

// AudioKit anteckning
// Triggerzon som sätter en parameter på MusicDirector (event-local)
// Param kan vara key (via AudioParamLibrary) eller direkt namn

namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("AudioKit/Zoner/Music Parameter Zone")]
    public sealed class MusicParameterZone : MonoBehaviour
    {
        [Header("Parameter")]
        [SerializeField] private AudioParamRef parameter;

        [Header("Värden")]
        [Range(0f, 1f)] [SerializeField] private float valueInside = 1f;
        [Range(0f, 1f)] [SerializeField] private float valueOutside = 0f;
        [SerializeField] private bool quantizedOnBar = false;

        [Header("Trigger")]
        [SerializeField] private string playerTag = "Player";

        // Legacy (gömda)
        [SerializeField, HideInInspector] private ParameterMode legacyMode = ParameterMode.Custom;
        [SerializeField, HideInInspector] private KnownMusicParam legacyKnown = KnownMusicParam.Intensity;
        [SerializeField, HideInInspector] private string legacyCustomParamName = "";
        [SerializeField, HideInInspector] private AudioParameterSO legacyParameter;

        private enum ParameterMode { Known, Custom, Asset }
        private enum KnownMusicParam { State, Intensity, Danger }

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col) col.isTrigger = true;
        }

        private string ResolveParamName()
        {
            var resolved = parameter.Resolve(AudioResources.ParamLibrary);
            if (!string.IsNullOrEmpty(resolved)) return resolved;

            if (legacyMode == ParameterMode.Asset && legacyParameter != null && legacyParameter.IsValid)
                return legacyParameter.fmodName;

            // Legacy "Known" läser numera via AudioParamLibrary (key -> FMOD-namn)
            // för att undvika hårdkodade fält i AudioConfig.
            if (legacyMode == ParameterMode.Known)
            {
                var lib = AudioResources.ParamLibrary;
                string key = legacyKnown switch
                {
                    KnownMusicParam.State => "MusicState",
                    KnownMusicParam.Intensity => "MusicIntensity",
                    KnownMusicParam.Danger => "MusicDanger",
                    _ => null
                };

                if (!string.IsNullOrEmpty(key))
                {
                    var name = lib != null ? lib.Resolve(key) : key;
                    return AudioParamUtil.Resolve(name);
                }
            }

            return AudioParamUtil.Resolve(legacyCustomParamName);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;

            var name = ResolveParamName();
            if (string.IsNullOrEmpty(name)) return;

            var md = MusicDirector.I;
            if (md == null) return;

            if (quantizedOnBar) md.SetParamQuantized(name, valueInside);
            else md.SetParam(name, valueInside);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;

            var name = ResolveParamName();
            if (string.IsNullOrEmpty(name)) return;

            var md = MusicDirector.I;
            if (md == null) return;

            if (quantizedOnBar) md.SetParamQuantized(name, valueOutside);
            else md.SetParam(name, valueOutside);
        }
    }
}
