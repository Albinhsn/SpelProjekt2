using UnityEngine;

// AudioKit anteckning
// Driver en parameter från 0..1 baserat på progress längs en "path"
// Bra för slide-line, rails, tunnlar, chase-korridorer, osv
// Sätt minst 2 punkter, fler punkter ger fler "steps" i kurvan

namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    [AddComponentMenu("AudioKit/Drivers/Path Progress Param Driver")]
    public sealed class PathProgressParamDriver : MonoBehaviour
    {
        [Header("Parameter")]
        [SerializeField] private AudioParamRef parameter;

        [Header("Mål")]
        [SerializeField] private Transform player;
        [SerializeField] private string playerTag = "Player";

        [Header("Punkter")]
        [Tooltip("Minst 2. Progress går från point[0] -> point[last]")]
        [SerializeField] private Transform[] points;

        [Header("Värde")]
        [SerializeField] private bool invert;
        [SerializeField] private float smoothTime = 0.10f;

        [Header("Update")]
        [SerializeField] private bool updateInFixed;

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = true;

        private string paramName;
        private float vel;
        private float current;

        private void Awake()
        {
            if (player == null && !string.IsNullOrEmpty(playerTag))
            {
                var go = GameObject.FindGameObjectWithTag(playerTag);
                if (go != null) player = go.transform;
            }
        }

        private void OnEnable()
        {
            paramName = parameter.Resolve(AudioResources.ParamLibrary);
        }

        private void Update()
        {
            if (updateInFixed) return;
            Tick(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (!updateInFixed) return;
            Tick(Time.fixedDeltaTime);
        }

        private void Tick(float dt)
        {
            if (player == null) return;
            if (points == null || points.Length < 2) return;
            if (string.IsNullOrEmpty(paramName)) return;

            float t = ComputeProgress01(player.position);
            if (invert) t = 1f - t;

            current = smoothTime > 0f
                ? Mathf.SmoothDamp(current, t, ref vel, smoothTime)
                : t;

            AudioSystem.I?.SetGlobalParam(paramName, current);
        }

        private float ComputeProgress01(Vector3 pos)
        {
            float total = 0f;
            for (int i = 0; i < points.Length - 1; i++)
            {
                var a = points[i];
                var b = points[i + 1];
                if (a == null || b == null) continue;
                total += Vector3.Distance(a.position, b.position);
            }

            if (total <= 0.0001f) return 0f;

            float bestDistSqr = float.PositiveInfinity;
            float bestAlong = 0f;

            float alongSoFar = 0f;
            for (int i = 0; i < points.Length - 1; i++)
            {
                var aT = points[i];
                var bT = points[i + 1];
                if (aT == null || bT == null) continue;

                var a = aT.position;
                var b = bT.position;

                var ab = b - a;
                float len = ab.magnitude;
                if (len <= 0.0001f) continue;

                var dir = ab / len;
                float proj = Vector3.Dot(pos - a, dir);
                float clamped = Mathf.Clamp(proj, 0f, len);

                var closest = a + dir * clamped;
                float distSqr = (pos - closest).sqrMagnitude;

                if (distSqr < bestDistSqr)
                {
                    bestDistSqr = distSqr;
                    bestAlong = alongSoFar + clamped;
                }

                alongSoFar += len;
            }

            return Mathf.Clamp01(bestAlong / total);
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;
            if (points == null || points.Length < 2) return;

            Gizmos.matrix = Matrix4x4.identity;

            for (int i = 0; i < points.Length; i++)
            {
                var p = points[i];
                if (p == null) continue;
                Gizmos.DrawWireSphere(p.position, 0.15f);
            }

            for (int i = 0; i < points.Length - 1; i++)
            {
                var a = points[i];
                var b = points[i + 1];
                if (a == null || b == null) continue;
                Gizmos.DrawLine(a.position, b.position);
            }
        }
    }
}
