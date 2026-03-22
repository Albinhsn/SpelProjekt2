using UnityEngine;

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

        [Header("Punkter")]
        [Tooltip("Minst 2. Progress går från point[0] -> point[last]")]
        [SerializeField] private Transform[] points;

        [Tooltip("Måste ha samma längd som Points. Ex: [0,1,2] för Low/Mid/High.")]
        [SerializeField] private float[] pointValues;

        [Header("Värde")]
        [SerializeField] private bool invert;
        [SerializeField] private float smoothTime = 0.10f;

        [Header("Teleport Snap")]
        [SerializeField] private bool snapOnTeleport = true;
        [SerializeField] private float teleportDistance = 3.5f;

        [Header("Stacked Path Priority")]
        [Tooltip("Om flera delar av pathen ligger ovanpå varandra i X/Z väljs den högsta delen under spelaren.")]
        [SerializeField] private bool preferHighestSegmentUnderPlayer = true;

        [Tooltip("Hur nära i X/Z spelaren måste vara ett segment för att det ska räknas som 'under/ovanför'.")]
        [SerializeField] private float stackedHorizontalThreshold = 1.5f;

        [Tooltip("Tillåt att segmentet ligger lite ovanför spelaren och ändå räknas.")]
        [SerializeField] private float maxAbovePlayerTolerance = 0.75f;

        [Header("Update")]
        [SerializeField] private bool updateInFixed;

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = true;

        private string paramName;
        private float vel;
        private float current;
        private bool initialized;
        private bool hasLastPlayerPos;
        private Vector3 lastPlayerPos;

        private void TryToFindPlayer()
        {
            if (player == null)
            {
                Player go = FindFirstObjectByType<Player>();
                if (go != null)
                {
                    player = go.transform;
                }
            }
        }

        private void Start()
        {
            TryToFindPlayer();
            paramName = parameter.Resolve(AudioResources.ParamLibrary);
            SnapToCurrentPlayerPosition();
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
        }

        private void Update()
        {
            TryToFindPlayer();
            if (updateInFixed) return;
            Tick(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (!updateInFixed) return;
            Tick(Time.fixedDeltaTime);
        }

        private void SnapToCurrentPlayerPosition()
        {
            if (player == null) return;
            if (points == null || points.Length < 2) return;
            if (pointValues == null || pointValues.Length != points.Length) return;
            if (string.IsNullOrEmpty(paramName)) return;

            current = ComputeValue(player.position);
            vel = 0f;
            initialized = true;

            lastPlayerPos = player.position;
            hasLastPlayerPos = true;

            AudioSystem.I?.SetGlobalParam(paramName, current);
        }

        private void Tick(float dt)
        {
            if (player == null) return;
            if (points == null || points.Length < 2) return;
            if (pointValues == null || pointValues.Length != points.Length) return;
            if (string.IsNullOrEmpty(paramName)) return;

            float target = ComputeValue(player.position);

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

            AudioSystem.I?.SetGlobalParam(paramName, current);

            lastPlayerPos = player.position;
            hasLastPlayerPos = true;
        }

private float ComputeValue(Vector3 pos)
{
    // 1) Räkna totallängd + segment-start längs pathen
    float total = 0f;
    float[] segmentStartAlong = new float[points.Length - 1];

    for (int i = 0; i < points.Length - 1; i++)
    {
        var a = points[i];
        var b = points[i + 1];
        segmentStartAlong[i] = total;

        if (a == null || b == null) continue;
        total += Vector3.Distance(a.position, b.position);
    }

    if (total <= 0.0001f)
        return pointValues[0];

    // 2) Först: använd point-ordningen som prioritet vid stackade delar
    if (preferHighestSegmentUnderPlayer)
    {
        float horizontalThresholdSqr = stackedHorizontalThreshold * stackedHorizontalThreshold;

        // Gå BAKLÄNGES så senare/högre segment vinner
        for (int i = points.Length - 2; i >= 0; i--)
        {
            var aT = points[i];
            var bT = points[i + 1];
            if (aT == null || bT == null) continue;

            Vector3 a = aT.position;
            Vector3 b = bT.position;

            Vector2 aXZ = new Vector2(a.x, a.z);
            Vector2 bXZ = new Vector2(b.x, b.z);
            Vector2 pXZ = new Vector2(pos.x, pos.z);

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
            alongPriority = Mathf.Clamp(alongPriority, 0f, total);

            float accPriority = 0f;
            for (int j = 0; j < points.Length - 1; j++)
            {
                var pa = points[j];
                var pb = points[j + 1];
                if (pa == null || pb == null) continue;

                float len = Vector3.Distance(pa.position, pb.position);
                if (len <= 0.0001f) continue;

                if (alongPriority <= accPriority + len || j == points.Length - 2)
                {
                    float segT = Mathf.Clamp01((alongPriority - accPriority) / len);
                    return Mathf.Lerp(pointValues[j], pointValues[j + 1], segT);
                }

                accPriority += len;
            }
        }
    }

    // 3) Fallback: vanlig närmaste punkt i 3D
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
    along = Mathf.Clamp(along, 0f, total);

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