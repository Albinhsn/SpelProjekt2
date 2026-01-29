using UnityEngine;

namespace SP2.Audio
{
    // Antingen:
    // - kalla SetPaused(true/false) fran UI
    // - eller lat den auto-detektera Time.timeScale == 0
    public sealed class PauseAudioBridge : MonoBehaviour
    {
        [SerializeField] private bool autoDetectTimeScale = true;
        [Min(0f)] [SerializeField] private float pollIntervalSeconds = 0.1f;

        [Header("== Effects ==")]
        [SerializeField] private bool togglePauseSnapshot = true;
        [SerializeField] private bool setGlobalParam = false;
        [SerializeField] private string globalParamName = "Paused";
        [SerializeField] private int priority = 200;
        [Min(0f)] [SerializeField] private float fadeSeconds = 0.1f;

        private bool _paused;
        private float _nextPoll;

        public void SetPaused(bool paused)
        {
            if (_paused == paused) return;
            _paused = paused;
            Apply();
        }

        private void Update()
        {
            if (!autoDetectTimeScale) return;

            if (pollIntervalSeconds > 0f && Time.unscaledTime < _nextPoll) return;
            _nextPoll = Time.unscaledTime + pollIntervalSeconds;

            bool isPaused = Time.timeScale <= 0.0001f;
            if (isPaused != _paused)
            {
                _paused = isPaused;
                Apply();
            }
        }

        private void Apply()
        {
            var sys = AudioSystem.Instance;
            if (sys == null) return;

            if (togglePauseSnapshot)
                sys.Mixer?.SetPauseSnapshot(_paused);

            if (setGlobalParam && sys.Params != null && !string.IsNullOrEmpty(globalParamName))
            {
                if (_paused)
                    sys.Params.SetRequest(this, globalParamName, 1f, priority, fadeSeconds, true);
                else
                    sys.Params.ClearRequest(this, globalParamName);
            }
        }

        private void OnDisable()
        {
            // Safety
            var sys = AudioSystem.Instance;
            if (sys == null) return;

            if (togglePauseSnapshot)
                sys.Mixer?.SetPauseSnapshot(false);

            if (setGlobalParam && sys.Params != null && !string.IsNullOrEmpty(globalParamName))
                sys.Params.ClearRequest(this, globalParamName);

            _paused = false;
        }
    }
}
