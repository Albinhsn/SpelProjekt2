using System.Collections.Generic;
using UnityEngine;

namespace SP2.Audio
{
    //
    // Use:
    // - RegisterCombatSource/UnregisterCombatSource from gameplay
    // - Or Enter/Exit to toggle
    //
    // Output:
    // - GlobalParam: cfg.combatParam (0/1) via AudioParameterDriver
    // - Optional: MusicState -> Combat / Explore
    [DefaultExecutionOrder(-30)]
    public sealed class CombatAudioState : MonoBehaviour
    {
        [Header("== Output ==")]
        [SerializeField] private bool driveGlobalCombatParam = true;
        [SerializeField] private bool driveMusicState = true;
        [SerializeField] private MusicState combatMusicState = MusicState.Combat;
        [SerializeField] private MusicState calmMusicState = MusicState.Explore;

        [Header("== Timing ==")]
        [Tooltip("Holds combat active this long after last source clears.")]
        [Min(0f)] [SerializeField] private float exitDelaySeconds = 2f;
        [Tooltip("Fade time for global param.")]
        [Min(0f)] [SerializeField] private float fadeSeconds = 0.25f;
        [SerializeField] private bool useUnscaledTime = false;

        [Header("== Priority ==")]
        [SerializeField] private int priority = 50;

        private readonly HashSet<int> _sources = new();
        private bool _active;
        private float _exitAt;

        public bool IsInCombat => _active;

        public void Enter() => RegisterCombatSource(this);
        public void Exit() => UnregisterCombatSource(this);

        public void RegisterCombatSource(Object source)
        {
            if (source == null) return;

            _sources.Add(source.GetInstanceID());
            SetActive(true);
        }

        public void UnregisterCombatSource(Object source)
        {
            if (source == null) return;

            _sources.Remove(source.GetInstanceID());

            if (_sources.Count == 0)
                BeginExitDelay();
        }

        public void ClearAllSources()
        {
            _sources.Clear();
            SetActive(false);
        }

        private void BeginExitDelay()
        {
            if (exitDelaySeconds <= 0f)
            {
                SetActive(false);
                return;
            }

            float now = useUnscaledTime ? Time.unscaledTime : Time.time;
            _exitAt = now + exitDelaySeconds;
        }

        private void Update()
        {
            if (!_active) return;
            if (_sources.Count > 0) return;

            float now = useUnscaledTime ? Time.unscaledTime : Time.time;
            if (now >= _exitAt)
                SetActive(false);
        }

        private void SetActive(bool active)
        {
            if (_active == active) return;
            _active = active;

            var sys = AudioSystem.Instance;
            if (sys == null || sys.Config == null) return;

            if (driveGlobalCombatParam && sys.Params != null)
            {
                string name = sys.Config.combatParam;
                if (!string.IsNullOrEmpty(name))
                {
                    if (active)
                        sys.Params.SetRequest(this, name, 1f, priority, fadeSeconds, useUnscaledTime);
                    else
                        sys.Params.ClearRequest(this, name);
                }
            }

            if (driveMusicState && sys.Music != null)
                sys.Music.SetState(active ? combatMusicState : calmMusicState);
        }

        private void OnDisable()
        {
            // Safety: avoid stuck combat when object is disabled.
            if (!_active) return;

            var sys = AudioSystem.Instance;
            if (sys != null && sys.Config != null && sys.Params != null && driveGlobalCombatParam)
                sys.Params.ClearRequest(this, sys.Config.combatParam);

            _sources.Clear();
            _active = false;
        }
    }
}
