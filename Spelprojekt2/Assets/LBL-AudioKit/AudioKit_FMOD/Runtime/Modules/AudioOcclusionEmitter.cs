using UnityEngine;

// AudioKit anteckning
// Enkel occlusion/obstruction för AudioEmitter.
// Raycast från listener till emitter. Vid träff sätts en event-parameter 0..1.
// Parameter-namn kan vara key (via AudioParamLibrary) eller direkt FMOD-namn.
// FMOD-eventet behöver ha en parameter som styr LPF/volym/spatial blend/etc.

namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioEmitter))]
    [AddComponentMenu("AudioKit/Spatial/Audio Occlusion Emitter")]
    public sealed class AudioOcclusionEmitter : MonoBehaviour
    {
        [Header("Parameter")]
        [SerializeField] private AudioParamRef occlusionParam;
        [SerializeField, Range(0f, 1f)] private float clearValue = 0f;
        [SerializeField, Range(0f, 1f)] private float occludedValue = 1f;
        [Tooltip("Hur snabbt parametern rör sig mot målvärdet (sekunder). 0 = direkt.")]
        [SerializeField] private float smoothSeconds = 0.08f;

        [Header("Raycast")]
        [SerializeField] private LayerMask occluderMask = ~0;
        [Tooltip("0 = vanlig raycast. >0 = spherecast med given radie.")]
        [SerializeField] private float sphereRadius = 0.12f;
        [SerializeField] private float maxDistance = 120f;
        [SerializeField] private bool ignoreTriggers = true;
        [Tooltip("Valfritt: använd en specifik listener istället för auto-find.")]
        [SerializeField] private Transform customListener;

        private AudioEmitter emitter;
        private float current;

        private void Awake()
        {
            emitter = GetComponent<AudioEmitter>();
            current = clearValue;
        }

        private void Update()
        {
            if (emitter == null) return;

            var listener = customListener != null ? customListener : AudioListenerLocator.GetListenerTransform();
            if (listener == null) return;

            var a = listener.position;
            var b = transform.position;
            var ab = b - a;
            var dist = ab.magnitude;

            if (dist < 0.001f)
            {
                Apply(clearValue);
                return;
            }

            dist = Mathf.Min(dist, Mathf.Max(0.01f, maxDistance));
            var dir = ab / ab.magnitude;

            var query = ignoreTriggers ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide;
            var hit = false;
            RaycastHit hitInfo;

            if (sphereRadius > 0.001f)
                hit = Physics.SphereCast(a, sphereRadius, dir, out hitInfo, dist, occluderMask, query);
            else
                hit = Physics.Raycast(a, dir, out hitInfo, dist, occluderMask, query);

            // Ignorera träff på egen collider-hierarki
            if (hit && hitInfo.collider != null && hitInfo.collider.transform.IsChildOf(transform))
                hit = false;

            var target = hit ? occludedValue : clearValue;

            if (smoothSeconds <= 0f)
                current = target;
            else
                current = Mathf.MoveTowards(current, target, Time.deltaTime / smoothSeconds);

            Apply(current);
        }

        private void Apply(float value)
        {
            if (!occlusionParam.HasSomething) return;

            var lib = AudioResources.ParamLibrary;
            var resolved = occlusionParam.Resolve(lib);
            resolved = AudioParamUtil.Resolve(resolved);
            if (string.IsNullOrEmpty(resolved)) return;

            emitter.SetParam(resolved, value);
        }
    }
}
