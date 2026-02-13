using UnityEngine;

// AudioKit anteckning
// Spelar stinger efter nivå
// Skydd mot spam
// Bra för combo eller chain


namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    public sealed class ComboStingerPlayer : MonoBehaviour
    {
        [SerializeField] private AudioCueSO[] stingers = new AudioCueSO[0];
        [SerializeField] private float minInterval = 0.15f;

        private float nextTime;

        public void PlayComboLevel(int level)
        {
            if (SfxDirector.I == null) return;
            if (stingers == null || stingers.Length == 0) return;

            var t = Time.unscaledTime;
            if (t < nextTime) return;
            nextTime = t + minInterval;

            var idx = Mathf.Clamp(level - 1, 0, stingers.Length - 1);
            var cue = stingers[idx];
            if (cue != null)
                SfxDirector.I.PlayCue(cue, transform.position);
        }
    }
}
