using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace SP2.Audio
{
    [DefaultExecutionOrder(-90)]
    public sealed class AudioMixer : MonoBehaviour
    {
        public float Master01 => _master;
        public float Music01 => _music;
        public float Sfx01 => _sfx;
        public float Ui01 => _ui;

        private AudioConfigSO _cfg;

        private VCA _vcaMusic;
        private VCA _vcaSfx;
        private VCA _vcaUi;
        private Bus _busMaster;

        private float _master, _music, _sfx, _ui;

        private EventInstance _pauseSnapshot;
        private bool _pauseSnapshotActive;

        private void Awake()
        {
            _cfg = AudioResources.Config;
            if (_cfg == null) return;

            _busMaster = RuntimeManager.GetBus(_cfg.busMaster);
            _vcaMusic = RuntimeManager.GetVCA(_cfg.vcaMusic);
            _vcaSfx = RuntimeManager.GetVCA(_cfg.vcaSfx);
            _vcaUi = RuntimeManager.GetVCA(_cfg.vcaUi);

            _master = PlayerPrefs.GetFloat(_cfg.prefMaster, _cfg.defaultMaster);
            _music = PlayerPrefs.GetFloat(_cfg.prefMusic, _cfg.defaultMusic);
            _sfx = PlayerPrefs.GetFloat(_cfg.prefSfx, _cfg.defaultSfx);
            _ui = PlayerPrefs.GetFloat(_cfg.prefUi, _cfg.defaultUi);

            ApplyAll();
        }

        private void OnDestroy()
        {
            // Snapshot cleanup
            if (_pauseSnapshot.isValid())
            {
                _pauseSnapshot.stop(STOP_MODE.ALLOWFADEOUT);
                _pauseSnapshot.release();
                _pauseSnapshot.clearHandle();
            }
        }

        public void SetMaster(float value01)
        {
            _master = Mathf.Clamp01(value01);
            ApplyMaster();
            Save(_cfg.prefMaster, _master);
        }

        public void SetMusic(float value01)
        {
            _music = Mathf.Clamp01(value01);
            ApplyMusic();
            Save(_cfg.prefMusic, _music);
        }

        public void SetSfx(float value01)
        {
            _sfx = Mathf.Clamp01(value01);
            ApplySfx();
            Save(_cfg.prefSfx, _sfx);
        }

        public void SetUi(float value01)
        {
            _ui = Mathf.Clamp01(value01);
            ApplyUi();
            Save(_cfg.prefUi, _ui);
        }

        private void Save(string key, float value)
        {
            if (string.IsNullOrEmpty(key)) return;
            PlayerPrefs.SetFloat(key, value);
        }

        private void ApplyAll()
        {
            ApplyMaster();
            ApplyMusic();
            ApplySfx();
            ApplyUi();
        }

        private void ApplyMaster()
        {
            if (_busMaster.isValid())
                _busMaster.setVolume(_master);
        }

        private void ApplyMusic()
        {
            if (_vcaMusic.isValid())
                _vcaMusic.setVolume(_music);
        }

        private void ApplySfx()
        {
            if (_vcaSfx.isValid())
                _vcaSfx.setVolume(_sfx);
        }

        private void ApplyUi()
        {
            if (_vcaUi.isValid())
                _vcaUi.setVolume(_ui);
        }

        public void SetPauseSnapshot(bool active)
        {
            if (_cfg == null) return;
            if (_cfg.pauseSnapshot.IsNull) return;

            if (active && !_pauseSnapshotActive)
            {
                if (!_pauseSnapshot.isValid())
                    _pauseSnapshot = RuntimeManager.CreateInstance(_cfg.pauseSnapshot);

                _pauseSnapshot.start();
                _pauseSnapshotActive = true;
            }
            else if (!active && _pauseSnapshotActive)
            {
                if (_pauseSnapshot.isValid())
                {
                    _pauseSnapshot.stop(STOP_MODE.ALLOWFADEOUT);
                    // behåll instansen för snabb toggling
                }

                _pauseSnapshotActive = false;
            }
        }
    }
}
