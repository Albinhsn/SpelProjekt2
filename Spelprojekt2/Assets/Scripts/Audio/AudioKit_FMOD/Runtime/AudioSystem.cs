using System.Collections;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

using STOP_MODE = global::FMOD.Studio.STOP_MODE;
// AudioKit anteckning
// Grundsystem för volym och snapshots
// Master via VCA eller bus fallback
// Globala parametrar går här


namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    public sealed class AudioSystem : MonoBehaviour
    {
        public static AudioSystem I { get; private set; }

        private AudioConfigSO cfg;

        private VCA vcaMaster;

        private VCA vcaMusic;
        private VCA vcaSfx;
        private VCA vcaUi;

        private Bus masterBus;

        private EventInstance pauseSnapshot;
        private EventInstance damageSnapshot;
        private bool pauseActive;

        private const string PrefMaster = "AUDIOKIT_VOL_MASTER";
        private const string PrefMusic = "AUDIOKIT_VOL_MUSIC";
        private const string PrefSfx = "AUDIOKIT_VOL_SFX";
        private const string PrefUi = "AUDIOKIT_VOL_UI";

        private void Awake()
        {
            if (I != null) { Destroy(gameObject); return; }
            I = this;

            cfg = AudioResources.Config;
            if (cfg == null) return;

            TryEnsureListener();

            vcaMaster = RuntimeManager.GetVCA(cfg.vcaMaster);
            vcaMusic = RuntimeManager.GetVCA(cfg.vcaMusic);
            vcaSfx = RuntimeManager.GetVCA(cfg.vcaSfx);
            vcaUi = RuntimeManager.GetVCA(cfg.vcaUi);

            masterBus = RuntimeManager.GetBus(cfg.masterBus);

            if (!cfg.pauseSnapshot.IsNull)
                pauseSnapshot = RuntimeManager.CreateInstance(cfg.pauseSnapshot);
            else if (!string.IsNullOrEmpty(cfg.pauseSnapshotPath))
                pauseSnapshot = RuntimeManager.CreateInstance(cfg.pauseSnapshotPath);

            if (!cfg.damageSnapshot.IsNull)
                damageSnapshot = RuntimeManager.CreateInstance(cfg.damageSnapshot);
            else if (!string.IsNullOrEmpty(cfg.damageSnapshotPath))
                damageSnapshot = RuntimeManager.CreateInstance(cfg.damageSnapshotPath);

            SetMasterVolume(PlayerPrefs.GetFloat(PrefMaster, cfg.masterDefault));
            SetMusicVolume(PlayerPrefs.GetFloat(PrefMusic, cfg.musicDefault));
            SetSfxVolume(PlayerPrefs.GetFloat(PrefSfx, cfg.sfxDefault));
            SetUiVolume(PlayerPrefs.GetFloat(PrefUi, cfg.uiDefault));
        }

        private void OnDestroy()
        {
            if (I != this) return;

            StopAndRelease(ref pauseSnapshot);
            StopAndRelease(ref damageSnapshot);

            I = null;
        }

        private void TryEnsureListener()
        {
            if (!cfg.ensureStudioListener) return;

            if (FindObjectOfType<StudioListener>() != null) return;

            var cam = Camera.main;
            if (cam != null)
                cam.gameObject.AddComponent<StudioListener>();
        }

        private static void StopAndRelease(ref EventInstance inst)
        {
            if (!inst.isValid()) return;
            inst.stop(STOP_MODE.ALLOWFADEOUT);
            inst.release();
            inst.clearHandle();
        }

        public void SetMasterVolume(float v)
        {
            v = Mathf.Clamp01(v);

            if (vcaMaster.isValid())
                vcaMaster.setVolume(v);
            else if (masterBus.isValid())
                masterBus.setVolume(v);

            PlayerPrefs.SetFloat(PrefMaster, v);
        }

        public void SetMusicVolume(float v)
        {
            v = Mathf.Clamp01(v);
            vcaMusic.setVolume(v);
            PlayerPrefs.SetFloat(PrefMusic, v);
        }

        public void SetSfxVolume(float v)
        {
            v = Mathf.Clamp01(v);
            vcaSfx.setVolume(v);
            PlayerPrefs.SetFloat(PrefSfx, v);
        }

        public void SetUiVolume(float v)
        {
            v = Mathf.Clamp01(v);
            vcaUi.setVolume(v);
            PlayerPrefs.SetFloat(PrefUi, v);
        }

        public void SetPauseSnapshot(bool active)
        {
            if (!pauseSnapshot.isValid()) return;
            if (pauseActive == active) return;

            pauseActive = active;

            if (active) pauseSnapshot.start();
            else pauseSnapshot.stop(STOP_MODE.ALLOWFADEOUT);
        }

        public void PulseDamageSnapshot(float holdSeconds)
        {
            if (!damageSnapshot.isValid()) return;
            StartCoroutine(DamagePulseRoutine(Mathf.Max(0.02f, holdSeconds)));
        }

        private IEnumerator DamagePulseRoutine(float hold)
        {
            damageSnapshot.start();
            yield return new WaitForSecondsRealtime(hold);
            damageSnapshot.stop(STOP_MODE.ALLOWFADEOUT);
        }

        public void SetGlobalParam(string name, float value)
        {
            if (string.IsNullOrEmpty(name)) return;
            RuntimeManager.StudioSystem.setParameterByName(name, value);
        }

        public void StopAllEvents(bool immediate)
        {
            if (!masterBus.isValid()) return;
            masterBus.stopAllEvents(immediate ? STOP_MODE.IMMEDIATE : STOP_MODE.ALLOWFADEOUT);
        }

        private void OnApplicationPause(bool pause)
        {
            SetPauseSnapshot(pause);
        }
    }
}
