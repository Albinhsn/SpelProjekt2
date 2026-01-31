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

        private Coroutine damageRoutine;
        private float damageStopAt;

        private const string PrefMaster = "AUDIOKIT_VOL_MASTER";
        private const string PrefMusic  = "AUDIOKIT_VOL_MUSIC";
        private const string PrefSfx    = "AUDIOKIT_VOL_SFX";
        private const string PrefUi     = "AUDIOKIT_VOL_UI";

        private void Awake()
        {
            // Singleton-guard
            if (I != null && I != this)
            {
                Destroy(gameObject);
                return;
            }

            // Config måste finnas innan vi sätter I
            cfg = AudioResources.Config;
            if (cfg == null)
            {
                Debug.LogError("[AudioSystem] AudioConfig saknas (Resources/Audio/AudioConfig).");
                Destroy(gameObject);
                return;
            }

            I = this;

            TryEnsureListener();

            // VCAs / Bus
            vcaMaster = RuntimeManager.GetVCA(cfg.vcaMaster);
            vcaMusic  = RuntimeManager.GetVCA(cfg.vcaMusic);
            vcaSfx    = RuntimeManager.GetVCA(cfg.vcaSfx);
            vcaUi     = RuntimeManager.GetVCA(cfg.vcaUi);

            masterBus = RuntimeManager.GetBus(cfg.masterBus);

            // Pause snapshot
            if (!cfg.pauseSnapshot.IsNull)
                pauseSnapshot = RuntimeManager.CreateInstance(cfg.pauseSnapshot);
            else if (!string.IsNullOrEmpty(cfg.pauseSnapshotPath))
                pauseSnapshot = RuntimeManager.CreateInstance(cfg.pauseSnapshotPath);

            // Damage snapshot
            if (!cfg.damageSnapshot.IsNull)
                damageSnapshot = RuntimeManager.CreateInstance(cfg.damageSnapshot);
            else if (!string.IsNullOrEmpty(cfg.damageSnapshotPath))
                damageSnapshot = RuntimeManager.CreateInstance(cfg.damageSnapshotPath);

            // Init volymer (sparade prefs -> annars defaults)
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

            if (AudioUnityFind.FindAny<StudioListener>(true) != null) return;

            var cam = Camera.main;
            if (cam == null) cam = AudioUnityFind.FindAny<Camera>(true);

            if (cam != null)
                cam.gameObject.AddComponent<StudioListener>();
            else
                Debug.LogWarning("[AudioSystem] Hittade ingen Camera att lägga StudioListener på.");
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

            if (vcaMusic.isValid())
                vcaMusic.setVolume(v);

            PlayerPrefs.SetFloat(PrefMusic, v);
        }

        public void SetSfxVolume(float v)
        {
            v = Mathf.Clamp01(v);

            if (vcaSfx.isValid())
                vcaSfx.setVolume(v);

            PlayerPrefs.SetFloat(PrefSfx, v);
        }

        public void SetUiVolume(float v)
        {
            v = Mathf.Clamp01(v);

            if (vcaUi.isValid())
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

        // Snapshot pulse där "senaste pulsen vinner"
        // Så du kan kalla PulseDamageSnapshot flera gånger utan att en tidig coroutine stänger av snapshot för tidigt.
        public void PulseDamageSnapshot(float holdSeconds)
        {
            if (!damageSnapshot.isValid()) return;

            var hold = Mathf.Max(0.02f, holdSeconds);
            damageStopAt = Mathf.Max(damageStopAt, Time.unscaledTime + hold);

            if (damageRoutine == null)
                damageRoutine = StartCoroutine(DamagePulseRoutine());
        }

        private IEnumerator DamagePulseRoutine()
        {
            damageSnapshot.start();

            while (Time.unscaledTime < damageStopAt)
                yield return null;

            damageSnapshot.stop(STOP_MODE.ALLOWFADEOUT);

            damageStopAt = 0f;
            damageRoutine = null;
        }

        public void SetGlobalParam(string name, float value)
        {
            if (string.IsNullOrEmpty(name)) return;
            RuntimeManager.StudioSystem.setParameterByName(name, value);
        }

        public void StopAllEvents(bool immediate)
        {
            if (!masterBus.isValid()) return;

            masterBus.stopAllEvents(
                immediate ? STOP_MODE.IMMEDIATE : STOP_MODE.ALLOWFADEOUT
            );
        }

        private void OnApplicationPause(bool pause)
        {
            SetPauseSnapshot(pause);
        }
    }
}
