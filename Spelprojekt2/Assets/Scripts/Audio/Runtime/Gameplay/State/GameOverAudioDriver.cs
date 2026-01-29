using UnityEngine;

namespace SP2.Audio
{
    // Kalla SetGameOver(true/false) fran gameplay.
    public sealed class GameOverAudioDriver : MonoBehaviour
    {
        [SerializeField] private bool setGlobalGameOverParam = true;
        [Min(0f)] [SerializeField] private float fadeSeconds = 0.5f;
        [SerializeField] private int priority = 999;
        [SerializeField] private bool useUnscaledTime = true;

        [Header("== Optional extras ==")]
        [SerializeField] private AudioCueSO gameOverStinger;
        [SerializeField] private bool setMusicStateOnGameOver = false;
        [SerializeField] private MusicState gameOverMusicState = MusicState.Menu;

        private bool _active;

        public void SetGameOver(bool on)
        {
            if (_active == on) return;
            _active = on;

            var sys = AudioSystem.Instance;
            if (sys == null || sys.Config == null) return;

            if (setGlobalGameOverParam && sys.Params != null)
            {
                string name = sys.Config.gameOverParam;
                if (!string.IsNullOrEmpty(name))
                {
                    if (on) sys.Params.SetRequest(this, name, 1f, priority, fadeSeconds, useUnscaledTime);
                    else sys.Params.ClearRequest(this, name);
                }
            }

            if (on)
            {
                sys.Sfx?.Play2D(gameOverStinger);

                if (setMusicStateOnGameOver)
                    sys.Music?.SetState(gameOverMusicState);
            }
        }

        private void OnDisable()
        {
            if (!_active) return;

            var sys = AudioSystem.Instance;
            if (sys == null || sys.Config == null || sys.Params == null) return;

            if (setGlobalGameOverParam)
                sys.Params.ClearRequest(this, sys.Config.gameOverParam);

            _active = false;
        }
    }
}
