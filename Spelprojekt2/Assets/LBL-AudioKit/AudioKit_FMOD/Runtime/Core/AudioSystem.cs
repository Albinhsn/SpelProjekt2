using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

using STOP_MODE = global::FMOD.Studio.STOP_MODE;

// AudioKit anteckning
// Grundsystem för volym, snapshots och globala parametrar
// Master via VCA eller bus fallback
// Basvolymer sparas i PlayerPrefs. Ducking-multipliers är runtime och sparas inte.

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

        // Basvolymer (sparas) + duck multipliers (runtime)
        private float baseMaster = 1f, baseMusic = 1f, baseSfx = 1f, baseUi = 1f;
        private float duckMaster = 1f, duckMusic = 1f, duckSfx = 1f, duckUi = 1f;

        private EventInstance pauseSnapshot;
        private EventInstance damageSnapshot;

        // Extra snapshots (för actions/triggers). Key = id/path
        private readonly Dictionary<string, EventInstance> extraSnapshots = new Dictionary<string, EventInstance>(8);

        // Extra VCA cache (för generiska actions). Key = path
        private readonly Dictionary<string, VCA> extraVcas = new Dictionary<string, VCA>(8);

        private bool pauseActive;

        private Coroutine damageRoutine;
        private float damageStopAt;

        private const string PrefMaster = "AUDIOKIT_VOL_MASTER";
        private const string PrefMusic  = "AUDIOKIT_VOL_MUSIC";
        private const string PrefSfx    = "AUDIOKIT_VOL_SFX";
        private const string PrefUi     = "AUDIOKIT_VOL_UI";

        private VCA GetVCA(string vca)
        {
            VCA result = new();
            try
            {
                result = RuntimeManager.GetVCA(vca);
            }catch(VCANotFoundException)
            {
                Debug.LogWarning($"Couldn't find vca {vca}");
            }
            return result;
        }

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

            // VCAs / Bus
            vcaMaster = GetVCA(cfg.vcaMaster);
            vcaMusic  = GetVCA(cfg.vcaMusic);
            vcaSfx    = GetVCA(cfg.vcaSfx);
            vcaUi     = GetVCA(cfg.vcaUi);

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

            // extra snapshots
            foreach (var kv in extraSnapshots)
            {
                var inst = kv.Value;
                if (!inst.isValid()) continue;
                inst.stop(STOP_MODE.ALLOWFADEOUT);
                inst.release();
            }
            extraSnapshots.Clear();
            extraVcas.Clear();

            I = null;
        }

        private static void StopAndRelease(ref EventInstance inst)
        {
            if (!inst.isValid()) return;

            inst.stop(STOP_MODE.ALLOWFADEOUT);
            inst.release();
            inst.clearHandle();
        }

        private void ApplyMaster()
        {
            var v = Mathf.Clamp01(baseMaster * duckMaster);

            if (vcaMaster.isValid())
                vcaMaster.setVolume(v);
            else if (masterBus.isValid())
                masterBus.setVolume(v);
        }

        private void ApplyMusic()
        {
            var v = Mathf.Clamp01(baseMusic * duckMusic);
            if (vcaMusic.isValid())
                vcaMusic.setVolume(v);
        }

        private void ApplySfx()
        {
            var v = Mathf.Clamp01(baseSfx * duckSfx);
            if (vcaSfx.isValid())
                vcaSfx.setVolume(v);
        }

        private void ApplyUi()
        {
            var v = Mathf.Clamp01(baseUi * duckUi);
            if (vcaUi.isValid())
                vcaUi.setVolume(v);
        }

        public float GetMasterVolume()
        {
            return baseMaster;
        }

        public void SetMasterVolume(float v)
        {
            baseMaster = Mathf.Clamp01(v);
            ApplyMaster();
            PlayerPrefs.SetFloat(PrefMaster, baseMaster);
        }

        public void SetMusicVolume(float v)
        {
            baseMusic = Mathf.Clamp01(v);
            ApplyMusic();
            PlayerPrefs.SetFloat(PrefMusic, baseMusic);
        }

        public void SetSfxVolume(float v)
        {
            baseSfx = Mathf.Clamp01(v);
            ApplySfx();
            PlayerPrefs.SetFloat(PrefSfx, baseSfx);
        }

        public void SetUiVolume(float v)
        {
            baseUi = Mathf.Clamp01(v);
            ApplyUi();
            PlayerPrefs.SetFloat(PrefUi, baseUi);
        }

        // Ducking: sätt en runtime-multiplikator (0..1) som multipliceras med basvolymen.
        // Detta sparas inte i PlayerPrefs.
        public void SetDuckMultiplier(string vcaAlias, float multiplier)
        {
            if (string.IsNullOrEmpty(vcaAlias)) return;
            multiplier = Mathf.Clamp01(multiplier);

            var a = vcaAlias.Trim();
            if (a.Equals("Master", System.StringComparison.OrdinalIgnoreCase)) { duckMaster = multiplier; ApplyMaster(); return; }
            if (a.Equals("Music",  System.StringComparison.OrdinalIgnoreCase)) { duckMusic  = multiplier; ApplyMusic();  return; }
            if (a.Equals("Sfx",    System.StringComparison.OrdinalIgnoreCase) ||
                a.Equals("SFX",    System.StringComparison.OrdinalIgnoreCase)) { duckSfx = multiplier; ApplySfx(); return; }
            if (a.Equals("UI",     System.StringComparison.OrdinalIgnoreCase)) { duckUi = multiplier; ApplyUi(); return; }
        }

        public float GetBaseVolume(string alias)
        {
            if (string.IsNullOrEmpty(alias)) return 1f;
            var a = alias.Trim();
            if (a.Equals("Master", System.StringComparison.OrdinalIgnoreCase)) return baseMaster;
            if (a.Equals("Music",  System.StringComparison.OrdinalIgnoreCase)) return baseMusic;
            if (a.Equals("Sfx",    System.StringComparison.OrdinalIgnoreCase) || a.Equals("SFX", System.StringComparison.OrdinalIgnoreCase)) return baseSfx;
            if (a.Equals("UI",     System.StringComparison.OrdinalIgnoreCase)) return baseUi;
            return 1f;
        }

        public float GetDuckMultiplier(string alias)
        {
            if (string.IsNullOrEmpty(alias)) return 1f;
            var a = alias.Trim();
            if (a.Equals("Master", System.StringComparison.OrdinalIgnoreCase)) return duckMaster;
            if (a.Equals("Music",  System.StringComparison.OrdinalIgnoreCase)) return duckMusic;
            if (a.Equals("Sfx",    System.StringComparison.OrdinalIgnoreCase) || a.Equals("SFX", System.StringComparison.OrdinalIgnoreCase)) return duckSfx;
            if (a.Equals("UI",     System.StringComparison.OrdinalIgnoreCase)) return duckUi;
            return 1f;
        }

        public float GetEffectiveVolume(string alias)
        {
            return Mathf.Clamp01(GetBaseVolume(alias) * GetDuckMultiplier(alias));
        }

        // Generisk VCA-sättare (för AudioActionType.SetVca)
        // Tips: använd VCA path (t.ex. "vca:/Music") eller alias "Master/Music/Sfx/UI".
        public void SetVca(string vcaPathOrAlias, float value)
        {
            if (string.IsNullOrEmpty(vcaPathOrAlias)) return;

            // Aliaser till config
            var alias = vcaPathOrAlias.Trim();
            if (alias.Equals("Master", System.StringComparison.OrdinalIgnoreCase)) { SetMasterVolume(value); return; }
            if (alias.Equals("Music",  System.StringComparison.OrdinalIgnoreCase)) { SetMusicVolume(value);  return; }
            if (alias.Equals("Sfx",    System.StringComparison.OrdinalIgnoreCase) ||
                alias.Equals("SFX",    System.StringComparison.OrdinalIgnoreCase)) { SetSfxVolume(value); return; }
            if (alias.Equals("UI",     System.StringComparison.OrdinalIgnoreCase)) { SetUiVolume(value);   return; }

            value = Mathf.Clamp01(value);

            if (!extraVcas.TryGetValue(alias, out var v) || !v.isValid())
            {
                v = RuntimeManager.GetVCA(alias);
                extraVcas[alias] = v;
            }

            if (v.isValid())
                v.setVolume(value);
        }

        // Generisk snapshot-kontroll (för AudioActionType.SetSnapshot)
        // snapshotIdOrPath kan vara:
        // - EventRegistry-id (om den finns)
        // - FMOD path (t.ex. "event:/Snapshots/Pause")
        public void SetSnapshot(string snapshotIdOrPath, bool enable, float fade)
        {
            if (string.IsNullOrEmpty(snapshotIdOrPath)) return;

            // Supporta registry-id
            EventInstance inst = default;
            var key = snapshotIdOrPath.Trim();

            // Specialfall: config snapshots
            if (cfg != null)
            {
                // Om man skickar "Pause" eller "Damage" som alias
                if (key.Equals("Pause", System.StringComparison.OrdinalIgnoreCase) && pauseSnapshot.isValid())
                {
                    if (enable) pauseSnapshot.start();
                    else pauseSnapshot.stop(fade > 0.001f ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
                    return;
                }
            }

            if (!extraSnapshots.TryGetValue(key, out inst) || !inst.isValid())
            {
                // Försök registry först
                var reg = AudioResources.EventRegistry;
                if (reg != null && reg.TryGetEvent(key, out var ev) && !ev.IsNull)
                {
                    inst = RuntimeManager.CreateInstance(ev);
                }
                else
                {
                    // Tolka som path
                    inst = RuntimeManager.CreateInstance(key);
                }

                extraSnapshots[key] = inst;
            }

            if (!inst.isValid()) return;

            if (enable)
            {
                inst.start();
            }
            else
            {
                inst.stop(fade > 0.001f ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
                // Behåll instansen i cache så man kan slå på igen (billigare). Vid behov: releasa här.
            }
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
        // Möjliggör att PulseDamageSnapshot kan anropas flera gånger utan att en tidig coroutine stänger av snapshot för tidigt.
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
            // key -> FMOD-namn (från AudioParamLibrary) eller direkt namn
            var lib = AudioResources.ParamLibrary;
            var resolved = lib != null ? lib.Resolve(name) : name;
            resolved = AudioParamUtil.Resolve(resolved);
            if (string.IsNullOrEmpty(resolved)) return;

            RuntimeManager.StudioSystem.setParameterByName(resolved, value);
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
