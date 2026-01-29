using UnityEngine;

namespace SP2.Audio
{
    public sealed class SpawnJinglePlayer : MonoBehaviour
    {
        [SerializeField] private AudioCueSO cue;
        [SerializeField] private bool play2D = false;
        [Min(0f)] [SerializeField] private float minIntervalSeconds = 0.25f;

        private float _lastTime;

        public void Play()
        {
            PlayAt(transform.position);
        }

        public void PlayAt(Vector3 worldPos)
        {
            if (cue == null) return;

            float now = Time.unscaledTime;
            if (minIntervalSeconds > 0f && now - _lastTime < minIntervalSeconds)
                return;

            _lastTime = now;

            var sys = AudioSystem.Instance;
            if (sys == null) return;

            if (play2D)
                sys.Sfx?.Play2D(cue);
            else
                sys.Sfx?.Play(cue, worldPos);
        }
    }
}
