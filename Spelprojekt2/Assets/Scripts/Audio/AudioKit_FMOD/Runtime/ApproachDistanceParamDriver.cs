using UnityEngine;

// AudioKit anteckning
// Global param från avstånd
// Nättare närmare ger högre värde
// Valfritt parameternamn


namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    public sealed class ApproachDistanceParamDriver : MonoBehaviour
    {
        public enum ParameterMode { Known, Custom, Asset }

        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Parameter")]
        [SerializeField] private ParameterMode mode = ParameterMode.Asset;

        // Behåller detta fält för gamla scenes
        [SerializeField] private KnownGlobalParam param = KnownGlobalParam.DangerZone;

        // Nytt för återanvändning
        [SerializeField] private string customParamName = "DangerZone";

        [Tooltip("Rekommenderat: använd en AudioParameterSO istället för att skriva namn.")] 
        [SerializeField] private AudioParameterSO parameter;

        [Header("Avstånd")]
        [SerializeField] private float nearDistance = 5f;
        [SerializeField] private float farDistance = 25f;

        [Header("Utdata")]
        [SerializeField] private bool invert;
        [SerializeField] private float smoothingSeconds = 0.10f;

        private string paramName;
        private float current;

        private void Start()
        {
            paramName = ResolveName();
        }

        private void Update()
        {
            if (AudioSystem.I == null) return;
            if (target == null) return;

            var name = ResolveName();
            if (string.IsNullOrEmpty(name)) return;

            var d = Vector3.Distance(transform.position, target.position);
            var t = Mathf.InverseLerp(farDistance, nearDistance, d);
            t = Mathf.Clamp01(t);

            if (invert) t = 1f - t;

            if (smoothingSeconds <= 0f) current = t;
            else current = Mathf.MoveTowards(current, t, Time.unscaledDeltaTime / smoothingSeconds);

            AudioSystem.I.SetGlobalParam(name, current);
        }

        private string ResolveName()
        {
            if (mode == ParameterMode.Asset) return (parameter != null && parameter.IsValid) ? parameter.fmodName : null;

            if (mode == ParameterMode.Custom) return customParamName;

            if (!string.IsNullOrEmpty(paramName)) return paramName;

            if (AudioParameterDriver.I == null) return null;
            return AudioParameterDriver.I.Resolve(param);
        }
    }
}
