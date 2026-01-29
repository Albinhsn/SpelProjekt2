using UnityEngine;

namespace SP2.Audio
{
    public enum ZoneTarget
    {
        GlobalParameter,
        MusicState,
        MusicIntensity,
        MusicDanger
    }

    // GlobalParameter går via AudioParameterDriver (prio + fade + overlap-safe)
    [RequireComponent(typeof(Collider))]
    public sealed class AudioParameterZone : MonoBehaviour
    {
        [Header("== Target ==")]
        [SerializeField] private ZoneTarget target = ZoneTarget.GlobalParameter;

        [Header("== MusicState ==")]
        [SerializeField] private MusicState musicState = MusicState.Explore;

        [Header("== Global Parameter ==")]
        [SerializeField] private KnownGlobalParam param = KnownGlobalParam.Combat;
        [SerializeField] private string customParamName = "";

        [Header("== Value / Fade ==")]
        [SerializeField] private float insideValue = 1f;
        [Min(0f)] [SerializeField] private float fadeSeconds = 0.25f;
        [SerializeField] private int priority = 0;
        [SerializeField] private bool useUnscaledTime = false;

        [Header("== Filter ==")]
        [Tooltip("Tom = alla colliders triggar. Annars bara objekt med denna Tag.")]
        [SerializeField] private string requiredTag = "Player";

        private Collider _col;
        private bool _inside;

        private void Reset()
        {
            _col = GetComponent<Collider>();
            if (_col != null) _col.isTrigger = true;
        }

        private void Awake()
        {
            _col = GetComponent<Collider>();
            _col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_inside) return;
            if (!PassesFilter(other)) return;

            _inside = true;
            ApplyEnter();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!_inside) return;
            if (!PassesFilter(other)) return;

            _inside = false;
            ApplyExit();
        }

        private bool PassesFilter(Collider other)
        {
            if (string.IsNullOrEmpty(requiredTag))
                return true;

            return other.CompareTag(requiredTag);
        }

        private void ApplyEnter()
        {
            var sys = AudioSystem.Instance;
            if (sys == null) return;

            switch (target)
            {
                case ZoneTarget.GlobalParameter:
                {
                    var driver = sys.GetComponent<AudioParameterDriver>();
                    if (driver == null) return;

                    string name = ResolveGlobalParamName(sys.Config);
                    driver.SetRequest(this, name, insideValue, priority, fadeSeconds, useUnscaledTime);
                    break;
                }

                case ZoneTarget.MusicState:
                    sys.Music?.SetState(musicState);
                    break;

                case ZoneTarget.MusicIntensity:
                    sys.Music?.SetIntensity(insideValue);
                    break;

                case ZoneTarget.MusicDanger:
                    sys.Music?.SetDanger(insideValue);
                    break;
            }
        }

        private void ApplyExit()
        {
            var sys = AudioSystem.Instance;
            if (sys == null) return;

            switch (target)
            {
                case ZoneTarget.GlobalParameter:
                {
                    var driver = sys.GetComponent<AudioParameterDriver>();
                    if (driver == null) return;

                    string name = ResolveGlobalParamName(sys.Config);
                    driver.ClearRequest(this, name);
                    break;
                }

                case ZoneTarget.MusicState:
                    // Ingen auto-reset, sköt via annat system
                    break;

                case ZoneTarget.MusicIntensity:
                    sys.Music?.SetIntensity(0f);
                    break;

                case ZoneTarget.MusicDanger:
                    sys.Music?.SetDanger(0f);
                    break;
            }
        }

        private string ResolveGlobalParamName(AudioConfigSO cfg)
        {
            if (cfg == null) return customParamName;

            return param switch
            {
                KnownGlobalParam.Combat => cfg.combatParam,
                KnownGlobalParam.Indoor => cfg.indoorParam,
                KnownGlobalParam.DangerZone => cfg.dangerZoneParam,
                KnownGlobalParam.GameOver => cfg.gameOverParam,
                KnownGlobalParam.Custom => customParamName,
                _ => customParamName
            };
        }
    }
}
