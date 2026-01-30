using UnityEngine;

// AudioKit anteckning
// Sätter en global param från avstånd
// Bra för spänning eller hintar
// Tänk nära högre värde


namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    public sealed class ApproachDistanceParamDriver : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private KnownGlobalParam param = KnownGlobalParam.DangerZone;

        [Header("Avstånd")]
        [SerializeField] private float nearDistance = 5f;
        [SerializeField] private float farDistance = 25f;

        private string paramName;

        private void Start()
        {
            if (AudioParameterDriver.I != null)
                paramName = AudioParameterDriver.I.Resolve(param);
        }

        private void Update()
        {
            if (AudioSystem.I == null) return;
            if (target == null) return;

            if (string.IsNullOrEmpty(paramName) && AudioParameterDriver.I != null)
                paramName = AudioParameterDriver.I.Resolve(param);

            var d = Vector3.Distance(transform.position, target.position);
            var t = Mathf.InverseLerp(farDistance, nearDistance, d);
            AudioSystem.I.SetGlobalParam(paramName, Mathf.Clamp01(t));
        }
    }
}
