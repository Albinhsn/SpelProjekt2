using System;
using UnityEngine;

namespace SP2.Audio
{
    [Serializable]
    public struct ComboTier
    {
        [Min(0)] public int minCombo;
        public AudioCueSO stinger;
    }

    // Kalla OnComboChanged(combo) från din combo-system.
    // När combo passerar en ny tier, spelas stinger.
    public sealed class ComboStingerPlayer : MonoBehaviour
    {
        [Header("== Tiers (sorteras av minCombo) ==")]
        [SerializeField] private ComboTier[] tiers;

        [Header("== Options ==")]
        [SerializeField] private bool play2D = true;
        [Min(0f)] [SerializeField] private float minIntervalSeconds = 0.1f;

        private int _currentTier;
        private float _lastPlayTime;

        private void Awake()
        {
            SortTiers();
        }

        private void SortTiers()
        {
            if (tiers == null || tiers.Length <= 1) return;
            Array.Sort(tiers, (a, b) => a.minCombo.CompareTo(b.minCombo));
        }

        public void ResetCombo()
        {
            _currentTier = 0;
        }

        public void OnComboChanged(int combo)
        {
            if (tiers == null || tiers.Length == 0) return;

            int tierIndex = ResolveTierIndex(combo);
            if (tierIndex <= _currentTier) return;

            if (minIntervalSeconds > 0f && Time.unscaledTime - _lastPlayTime < minIntervalSeconds)
                return;

            _currentTier = tierIndex;
            _lastPlayTime = Time.unscaledTime;

            var cue = tiers[tierIndex].stinger;
            if (cue == null) return;

            var sys = AudioSystem.Instance;
            if (sys == null) return;

            if (play2D)
                sys.Sfx?.Play2D(cue);
            else
                sys.Sfx?.Play(cue, transform.position);
        }

        private int ResolveTierIndex(int combo)
        {
            int best = 0;
            for (int i = 0; i < tiers.Length; i++)
            {
                if (combo >= tiers[i].minCombo)
                    best = i;
            }
            return best;
        }
    }
}
