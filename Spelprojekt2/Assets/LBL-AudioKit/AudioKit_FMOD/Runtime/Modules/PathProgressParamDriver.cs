using UnityEngine;

namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    [AddComponentMenu("AudioKit/Drivers/Path Progress Param Driver")]
    public sealed class PathProgressParamDriver : MonoBehaviour
    {
        [System.Serializable]
        private sealed class FixedZoneOverride
        {
            public string label = "Zone";
            public BoxCollider zone;
            public float value = 2.2f;
            public int priority = 0;
        }

        [Header("Parameter")]
        [SerializeField] private AudioParamRef parameter;

        [Header("Priority Driver")]
        [SerializeField] private string sourceId = "PathProgress";
        [SerializeField] private int priority = 100;
        [SerializeField] private float fadeSeconds = 0f;

        [Header("Mål")]
        [SerializeField] private Transform player;

        [Header("Punkter")]
        [Tooltip("Minst 2. Progress går från point[0] -> point[last]")]
        [SerializeField] private Transform[] points;

        [Tooltip("Måste ha samma längd som Points. Ex: [0,1,2] för Low/Mid/High.")]
        [SerializeField] private float[] pointValues;

        [Header("Värde")]
        [SerializeField] private bool invert;
        [SerializeField] private float smoothTime = 0.25f;

        [Header("Teleport Snap")]
        [SerializeField] private bool snapOnTeleport = true;
        [SerializeField] private float teleportDistance = 3.5f;

        [Header("Stacked Path Priority")]
        [Tooltip("När flera delar av banan ligger nära samma X/Z prioriteras senare segment i listan först.")]
        [SerializeField] private bool preferHighestSegmentUnderPlayer = true;

        [Tooltip("Hur nära i X/Z spelaren måste vara ett segment för att det ska räknas som relevant.")]
        [SerializeField] private float stackedHorizontalThreshold = 1.75f;

        [Tooltip("Tillåt att segmentet ligger lite ovanför spelaren och ändå räknas.")]
        [SerializeField] private float maxAbovePlayerTolerance = 1.0f;

        [Header("Fixed Value Zones")]
        [SerializeField] private bool useFixedZones = true;
        [Tooltip("Om spelaren är inne i någon av dessa zoner används det fasta värdet istället för path-värdet.")]
        [SerializeField] private FixedZoneOverride[] fixedZones;

        [Header("Update")]
        [SerializeField] private bool updateInFixed;

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = true;

        private string paramName;
        private string runtimeSourceId;
        private float vel;
        private float current;
        private bool initialized;
        private bool hasLastPlayerPos;
        private Vector3 lastPlayerPos;

        private void Start()
        {
            TryToFindPlayer();
            EnsureResolved();
            SnapToCurrentPlayerPosition();
        }

        private void OnDisable()
        {
            EnsureResolved();

            if (AudioParameterDriver.I != null &&
                !string.IsNullOrEmpty(runtimeSourceId) &&
                !string.IsNullOrEmpty(paramName))
            {
                AudioParameterDriver.I.SetSourceActive(runtimeSourceId, paramName, current, false, priority, fadeSeconds);
            }
        }

        private void OnValidate()
        {
            int n = points != null ? points.Length : 0;
            if (n < 0) n = 0;

            if (pointValues == null || pointValues.Length != n)
            {
                var newVals = new float[n];
                for (int i = 0; i < n; i++) newVals[i] = i;
                pointValues = newVals;
            }

            stackedHorizontalThreshold = Mathf.Max(0.01f, stackedHorizontalThreshold);
            maxAbovePlayerTolerance = Mathf.Max(0f, maxAbovePlayerTolerance);
            teleportDistance = Mathf.Max(0f, teleportDistance);
            smoothTime = Mathf.Max(0f, smoothTime);
            fadeSeconds = Mathf.Max(0f, fadeSeconds);
        }

        private void Update()
        {
            TryToFindPlayer();
            if (updateInFixed) return;
            Tick();
        }

        private void FixedUpdate()
        {
            if (!updateInFixed) return;
            Tick();
        }

        private void TryToFindPlayer()
        {
            if (player == null)
            {
                Player go = FindFirstObjectByType<Player>();
                if (go != null)
                    player = go.transform;
            }
        }

        private void EnsureResolved()
        {
            if (string.IsNullOrEmpty(paramName))
                paramName = parameter.Resolve(AudioResources.ParamLibrary);

            if (string.IsNullOrWhiteSpace(runtimeSourceId))
                runtimeSourceId = $"{sourceId}_{GetInstanceID()}";
        }

        private void PushValue(float value)
        {
            EnsureResolved();
            if (string.IsNullOrEmpty(paramName)) return;

            if (AudioParameterDriver.I != null)
            {
                AudioParameterDriver.I.SetSourceActive(runtimeSourceId, paramName, value, true, priority, fadeSeconds);
            }
            else
            {
                AudioSystem.I?.SetGlobalParam(paramName, value);
            }
        }

        private void SnapToCurrentPlayerPosition()
        {
            EnsureResolved();

            if (player == null) return;
            if (points == null || points.Length < 2) return;
            if (pointValues == null || pointValues.Length != points.Length) return;
            if (string.IsNullOrEmpty(paramName)) return;

            current = GetTargetValue(player.position);
            vel = 0f;
            initialized = true;

            lastPlayerPos = player.position;
            hasLastPlayerPos = true;

            PushValue(current);
        }

        private void Tick()
        {
            EnsureResolved();

            if (player == null) return;
            if (points == null || points.Length < 2) return;
            if (pointValues == null || pointValues.Length != points.Length) return;
            if (string.IsNullOrEmpty(paramName)) return;

            float target = GetTargetValue(player.position);

            bool teleported = false;
            if (snapOnTeleport && hasLastPlayerPos)
            {
                float sqrDist = (player.position - lastPlayerPos).sqrMagnitude;
                teleported = sqrDist >= teleportDistance * teleportDistance;
            }

            if (!initialized || teleported)
            {
                current = target;
                vel = 0f;
                initialized = true;
            }
            else
            {
                current = smoothTime > 0f
                    ? Mathf.SmoothDamp(current, target, ref vel, smoothTime)
                    : target;
            }

            PushValue(current);

            lastPlayerPos = player.position;
            hasLastPlayerPos = true;
        }

        private float GetTargetValue(Vector3 pos)
        {
            if (TryGetFixedZoneValue(pos, out float zoneValue))
                return zoneValue;

            return ComputePathValue(pos);
        }

        private bool TryGetFixedZoneValue(Vector3 pos, out float value)
        {
            value = 0f;

            if (!useFixedZones || fixedZones == null || fixedZones.Length == 0)
                return false;

            bool found = false;
            int bestPriority = int.MinValue;
            int bestIndex = -1;
            float bestValue = 0f;

            for (int i = 0; i < fixedZones.Length; i++)
            {
                FixedZoneOverride z = fixedZones[i];
                if (z == null || z.zone == null) continue;
                if (!z.zone.enabled || !z.zone.gameObject.activeInHierarchy) continue;
                if (!IsInsideZone(z.zone, pos)) continue;

                bool better =
                    !found ||
                    z.priority > bestPriority ||
                    (z.priority == bestPriority && i > bestIndex);

                if (better)
                {
                    found = true;
                    bestPriority = z.priority;
                    bestIndex = i;
                    bestValue = z.value;
                }
            }

            if (!found)
                return false;

            value = bestValue;
            return true;
        }

        private static bool IsInsideZone(BoxCollider zone, Vector3 worldPos)
        {
            Vector3 closest = zone.ClosestPoint(worldPos);
            return (closest - worldPos).sqrMagnitude <= 0.000001f;
        }

        private float ComputePathValue(Vector3 pos)
        {
            float total = 0f;
            float[] segmentStartAlong = new float[points.Length - 1];

            for (int i = 0; i < points.Length - 1; i++)
            {
                var a = points[i];
                var b = points[i + 1];

                segmentStartAlong[i] = total;

                if (a == null || b == null)
                    continue;

                total += Vector3.Distance(a.position, b.position);
            }

            if (total <= 0.0001f)
                return pointValues[0];

            if (preferHighestSegmentUnderPlayer)
            {
                float horizontalThresholdSqr = stackedHorizontalThreshold * stackedHorizontalThreshold;
                Vector2 pXZ = new Vector2(pos.x, pos.z);

                for (int i = points.Length - 2; i >= 0; i--)
                {
                    var aT = points[i];
                    var bT = points[i + 1];
                    if (aT == null || bT == null) continue;

                    Vector3 a = aT.position;
                    Vector3 b = bT.position;

                    Vector2 aXZ = new Vector2(a.x, a.z);
                    Vector2 bXZ = new Vector2(b.x, b.z);
                    Vector2 abXZ = bXZ - aXZ;

                    float abXZLenSqr = abXZ.sqrMagnitude;
                    if (abXZLenSqr <= 0.0001f) continue;

                    float tXZ = Mathf.Clamp01(Vector2.Dot(pXZ - aXZ, abXZ) / abXZLenSqr);
                    Vector3 closest = Vector3.Lerp(a, b, tXZ);

                    Vector2 closestXZ = new Vector2(closest.x, closest.z);
                    float horizontalDistSqr = (pXZ - closestXZ).sqrMagnitude;

                    if (horizontalDistSqr > horizontalThresholdSqr)
                        continue;

                    if (closest.y > pos.y + maxAbovePlayerTolerance)
                        continue;

                    float segLen = Vector3.Distance(a, b);
                    float chosenAlong = segmentStartAlong[i] + (segLen * tXZ);
                    float alongPriority = invert ? (total - chosenAlong) : chosenAlong;

                    return SampleValueAtAlong(Mathf.Clamp(alongPriority, 0f, total));
                }
            }

            float bestDistSqr = float.PositiveInfinity;
            float bestAlong = 0f;
            float alongSoFar = 0f;

            for (int i = 0; i < points.Length - 1; i++)
            {
                var aT = points[i];
                var bT = points[i + 1];
                if (aT == null || bT == null) continue;

                Vector3 a = aT.position;
                Vector3 b = bT.position;

                Vector3 ab = b - a;
                float len = ab.magnitude;
                if (len <= 0.0001f) continue;

                Vector3 dir = ab / len;
                float proj = Vector3.Dot(pos - a, dir);
                float clamped = Mathf.Clamp(proj, 0f, len);

                Vector3 closest = a + dir * clamped;
                float distSqr = (pos - closest).sqrMagnitude;

                if (distSqr < bestDistSqr)
                {
                    bestDistSqr = distSqr;
                    bestAlong = alongSoFar + clamped;
                }

                alongSoFar += len;
            }

            float along = invert ? (total - bestAlong) : bestAlong;
            return SampleValueAtAlong(Mathf.Clamp(along, 0f, total));
        }

        private float SampleValueAtAlong(float along)
        {
            float acc = 0f;

            for (int i = 0; i < points.Length - 1; i++)
            {
                var a = points[i];
                var b = points[i + 1];
                if (a == null || b == null) continue;

                float len = Vector3.Distance(a.position, b.position);
                if (len <= 0.0001f) continue;

                if (along <= acc + len || i == points.Length - 2)
                {
                    float segT = Mathf.Clamp01((along - acc) / len);
                    return Mathf.Lerp(pointValues[i], pointValues[i + 1], segT);
                }

                acc += len;
            }

            return pointValues[pointValues.Length - 1];
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;

            Gizmos.matrix = Matrix4x4.identity;

            if (points != null && points.Length >= 2)
            {
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

            if (useFixedZones && fixedZones != null)
            {
                for (int i = 0; i < fixedZones.Length; i++)
                {
                    var z = fixedZones[i];
                    if (z == null || z.zone == null) continue;

                    Gizmos.matrix = z.zone.transform.localToWorldMatrix;
                    Gizmos.DrawWireCube(z.zone.center, z.zone.size);
                }

                Gizmos.matrix = Matrix4x4.identity;
            }
        }
    }
}