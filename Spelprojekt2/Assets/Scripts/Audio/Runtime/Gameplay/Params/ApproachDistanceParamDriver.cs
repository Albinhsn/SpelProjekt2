using UnityEngine;

namespace SP2.Audio
{
    // Ex: PreBossDistance, DangerZone, IndoorProximity, osv.
    //
    // Varde:
    // - 0 vid farDistance
    // - 1 vid nearDistance
    //
    // Satter request via AudioParameterDriver med prio + fade.
    [DefaultExecutionOrder(-25)]
    public sealed class ApproachDistanceParamDriver : MonoBehaviour
    {
        [Header("== Targets ==")]
        [SerializeField] private Transform listener; // player
        [SerializeField] private Transform target;   // boss/obj
        [SerializeField] private string listenerTag = "Player";

        [Header("== Global Parameter ==")]
        [SerializeField] private KnownGlobalParam param = KnownGlobalParam.DangerZone;
        [SerializeField] private string customParamName = "";

        [Header("== Distance (meters) ==")]
        [Min(0.01f)] [SerializeField] private float nearDistance = 5f;
        [Min(0.01f)] [SerializeField] private float farDistance = 25f;
        [SerializeField] private bool invert = false;

        [Header("== Driver ==")]
        [SerializeField] private bool active = true;
        [Min(0f)] [SerializeField] private float fadeSeconds = 0.15f;
        [SerializeField] private int priority = 10;
        [SerializeField] private bool useUnscaledTime = false;

        [Header("== Update rate ==")]
        [Min(0f)] [SerializeField] private float updateInterval = 0.05f; // 0 = every frame

        private float _nextUpdate;

        public void SetActive(bool on)
        {
            active = on;
            if (!active)
                ClearRequest();
        }

        private void Awake()
        {
            if (listener == null && !string.IsNullOrEmpty(listenerTag))
            {
                var go = GameObject.FindGameObjectWithTag(listenerTag);
                if (go != null) listener = go.transform;
            }
        }

        private void OnDisable()
        {
            ClearRequest();
        }

        private void Update()
        {
            if (!active) return;
            if (listener == null || target == null) return;

            if (updateInterval > 0f)
            {
                float now = useUnscaledTime ? Time.unscaledTime : Time.time;
                if (now < _nextUpdate) return;
                _nextUpdate = now + updateInterval;
            }

            float dist = Vector3.Distance(listener.position, target.position);

            float t;
            if (Mathf.Approximately(farDistance, nearDistance))
                t = 1f;
            else
                t = Mathf.InverseLerp(farDistance, nearDistance, dist); // 0 at far, 1 at near

            float v = invert ? 1f - Mathf.Clamp01(t) : Mathf.Clamp01(t);
            SetRequest(v);
        }

        private void SetRequest(float value01)
        {
            var sys = AudioSystem.Instance;
            if (sys == null || sys.Config == null || sys.Params == null) return;

            string name = ResolveParamName(sys.Config);
            if (string.IsNullOrEmpty(name)) return;

            sys.Params.SetRequest(this, name, value01, priority, fadeSeconds, useUnscaledTime);
        }

        private void ClearRequest()
        {
            var sys = AudioSystem.Instance;
            if (sys == null || sys.Config == null || sys.Params == null) return;

            string name = ResolveParamName(sys.Config);
            if (string.IsNullOrEmpty(name)) return;

            sys.Params.ClearRequest(this, name);
        }

        private string ResolveParamName(AudioConfigSO cfg)
        {
            if (param == KnownGlobalParam.Custom)
                return customParamName;

            if (cfg == null)
                return customParamName;

            return param switch
            {
                KnownGlobalParam.Combat => cfg.combatParam,
                KnownGlobalParam.Indoor => cfg.indoorParam,
                KnownGlobalParam.DangerZone => cfg.dangerZoneParam,
                KnownGlobalParam.GameOver => cfg.gameOverParam,
                _ => customParamName
            };
        }
    }
}
