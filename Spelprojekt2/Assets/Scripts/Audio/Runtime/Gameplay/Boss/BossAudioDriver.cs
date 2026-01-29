using UnityEngine;

namespace SP2.Audio
{
    // Koppla detta till din boss-controller och kalla:
    // - StartBossFight()
    // - SetPhase(1..maxPhases)
    // - EndBossFight()
    //
    // Den kan:
    // - Satta MusicState till Boss
    // - Spela stingers
    // - (valfritt) halla CombatParam = 1 under fighten
    // - (valfritt) aktivera en ApproachDistanceParamDriver
    [DefaultExecutionOrder(-20)]
    public sealed class BossAudioDriver : MonoBehaviour
    {
        [Header("== Optional drivers ==")]
        [SerializeField] private ApproachDistanceParamDriver approachDriver;

        [Header("== Music ==")]
        [SerializeField] private bool setMusicStateOnStart = true;
        [SerializeField] private MusicState bossMusicState = MusicState.Boss;
        [SerializeField] private bool restoreMusicStateOnEnd = true;
        [SerializeField] private MusicState restoreMusicState = MusicState.Explore;

        [Header("== Phase -> Intensity ==")]
        [SerializeField] private bool driveIntensityByPhase = true;
        [Min(1)] [SerializeField] private int maxPhases = 3;

        [Header("== Stingers ==")]
        [SerializeField] private AudioCueSO bossStartStinger;
        [SerializeField] private AudioCueSO bossEndStinger;
        [SerializeField] private AudioCueSO[] phaseStingers;

        [Header("== Optional: Combat global param ==")]
        [SerializeField] private bool holdCombatParam = true;
        [SerializeField] private int combatPriority = 80;
        [Min(0f)] [SerializeField] private float combatFadeSeconds = 0.25f;
        [SerializeField] private bool useUnscaledTime = false;

        private bool _active;
        private int _phase;

        public bool IsActive => _active;
        public int Phase => _phase;

        public void StartBossFight()
        {
            if (_active) return;
            _active = true;

            var sys = AudioSystem.Instance;
            if (sys == null || sys.Config == null) return;

            if (approachDriver != null)
                approachDriver.SetActive(true);

            if (holdCombatParam && sys.Params != null)
            {
                string p = sys.Config.combatParam;
                if (!string.IsNullOrEmpty(p))
                    sys.Params.SetRequest(this, p, 1f, combatPriority, combatFadeSeconds, useUnscaledTime);
            }

            if (setMusicStateOnStart)
                sys.Music?.SetState(bossMusicState);

            sys.Sfx?.Play2D(bossStartStinger);

            SetPhase(1);
        }

        public void EndBossFight()
        {
            if (!_active) return;
            _active = false;

            var sys = AudioSystem.Instance;
            if (sys == null || sys.Config == null) return;

            if (approachDriver != null)
                approachDriver.SetActive(false);

            if (holdCombatParam && sys.Params != null)
            {
                string p = sys.Config.combatParam;
                if (!string.IsNullOrEmpty(p))
                    sys.Params.ClearRequest(this, p);
            }

            if (restoreMusicStateOnEnd)
                sys.Music?.SetState(restoreMusicState);

            if (driveIntensityByPhase)
                sys.Music?.SetIntensity(0f);

            sys.Sfx?.Play2D(bossEndStinger);
        }

        // phase: 1..maxPhases
        public void SetPhase(int phase)
        {
            if (!_active) return;

            int clamped = Mathf.Clamp(phase, 1, Mathf.Max(1, maxPhases));
            if (_phase == clamped) return;
            _phase = clamped;

            var sys = AudioSystem.Instance;
            if (sys == null) return;

            if (driveIntensityByPhase)
            {
                float t = (maxPhases <= 1) ? 1f : (float)(_phase - 1) / (maxPhases - 1);
                sys.Music?.SetIntensity(t);
            }

            // phase stinger
            if (phaseStingers != null)
            {
                int idx = Mathf.Clamp(_phase - 1, 0, phaseStingers.Length - 1);
                if (idx >= 0 && idx < phaseStingers.Length)
                    sys.Sfx?.Play2D(phaseStingers[idx]);
            }
        }

        private void OnDisable()
        {
            // Safety
            if (_active)
                EndBossFight();
        }
    }
}
