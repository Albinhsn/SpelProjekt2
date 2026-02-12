using UnityEngine;

// AudioKit anteckning
// Driver en global parameter baserat på avstånd till en target
// Ex: när spelaren närmar sig en zon, en slide-line start, en portal, osv

namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    [AddComponentMenu("AudioKit/Drivers/Approach Distance Param Driver")]
    public sealed class ApproachDistanceParamDriver : MonoBehaviour
    {
        [Header("Parameter")]
        [SerializeField] private AudioParamRef parameter;

        [Header("Målpunkter")]
        [SerializeField] private Transform target;
        [SerializeField] private Transform player;
        [SerializeField] private string playerTag = "Player";

        [Header("Avstånd")]
        [SerializeField] private float minDistance = 1f;
        [SerializeField] private float maxDistance = 25f;
        [SerializeField] private bool invert = false;

        [Header("Smoothing")]
        [SerializeField] private float lerpSpeed = 12f;

        // Legacy (gömda)
        [SerializeField, HideInInspector] private KnownGlobalParam legacyKnownParam = KnownGlobalParam.Custom;
        [SerializeField, HideInInspector] private string legacyCustomParamName = "";

        private float current;

        private void Reset()
        {
            minDistance = 1f;
            maxDistance = 25f;
            lerpSpeed = 12f;
        }

        private void Awake()
        {
            if (player == null)
            {
                var go = GameObject.FindGameObjectWithTag(playerTag);
                if (go != null) player = go.transform;
            }
        }

        private void Update()
        {
            if (target == null) return;
            if (player == null) return;
            if (AudioSystem.I == null) return;

            var name = ResolveParamName();
            if (string.IsNullOrEmpty(name)) return;

            var dist = Vector3.Distance(player.position, target.position);
            var t = Mathf.InverseLerp(maxDistance, minDistance, dist);
            if (invert) t = 1f - t;

            var speed = lerpSpeed <= 0f ? 1f : lerpSpeed;
            current = Mathf.Lerp(current, t, Time.deltaTime * speed);

            AudioSystem.I.SetGlobalParam(name, current);
        }

        private string ResolveParamName()
        {
            var resolved = parameter.Resolve(AudioResources.ParamLibrary);
            if (!string.IsNullOrEmpty(resolved)) return resolved;

            if (legacyKnownParam != KnownGlobalParam.Custom)
                return AudioParameterDriver.I != null ? AudioParameterDriver.I.Resolve(legacyKnownParam) : null;

            return AudioParamUtil.Resolve(legacyCustomParamName);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (target == null) return;
            Gizmos.DrawWireSphere(target.position, minDistance);
            Gizmos.DrawWireSphere(target.position, maxDistance);
        }
#endif
    }
}
