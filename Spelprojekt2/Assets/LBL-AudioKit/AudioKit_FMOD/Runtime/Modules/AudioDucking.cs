using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

// AudioKit anteckning
// Kodbaserad ducking som kan användas i alla genrer:
// - För UI/beeps som ska duck:a music
// - För voiceover som duck:ar SFX
// Kombinerar flera duck-källor via token-system.

namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    [AddComponentMenu("AudioKit/Mix/Audio Ducking")]
    public sealed class AudioDucking : MonoBehaviour
    {
        public static AudioDucking I { get; private set; }

        [Header("Target")]
        [Tooltip("Alias: Master, Music, Sfx, UI. Eller en direkt VCA-path (vca:/...).")]
        [SerializeField] private string target = "Music";

        [Header("Smoothing")]
        [Tooltip("Sekunder för att gå mot mer ducking.")]
        [SerializeField] private float attack = 0.05f;

        [Tooltip("Sekunder för att släppa ducking.")]
        [SerializeField] private float release = 0.20f;

        [Header("Debug")]
        [SerializeField] private bool log;

        private readonly Dictionary<string, float> tokens = new Dictionary<string, float>(16);

        private float currentDuck;
        private float targetDuck;

        // För direkt VCA-path
        private VCA directVca;
        private float directBase = 1f;
        private bool directInit;

        public float CurrentDuck => currentDuck;
        public float TargetDuck => targetDuck;

        private void Awake()
        {
            if (I != null && I != this)
            {
                Destroy(gameObject);
                return;
            }
            I = this;

            TryInitDirectVca();
        }

        private void OnDestroy()
        {
            if (I == this) I = null;
        }

        private void Update()
        {
            // hitta max duck
            float max = 0f;
            foreach (var kv in tokens)
                if (kv.Value > max) max = kv.Value;

            targetDuck = Mathf.Clamp01(max);

            if (targetDuck > currentDuck)
            {
                if (attack <= 0f) currentDuck = targetDuck;
                else currentDuck = Mathf.MoveTowards(currentDuck, targetDuck, Time.unscaledDeltaTime / attack);
            }
            else
            {
                if (release <= 0f) currentDuck = targetDuck;
                else currentDuck = Mathf.MoveTowards(currentDuck, targetDuck, Time.unscaledDeltaTime / release);
            }

            var mult = 1f - currentDuck;
            Apply(mult);
        }

        public void Begin(string token, float duckAmount01)
        {
            if (string.IsNullOrEmpty(token)) token = "default";
            tokens[token] = Mathf.Clamp01(duckAmount01);

            if (log) Debug.Log($"[AudioDucking] Begin {token} = {duckAmount01:0.00}");
        }

        public void End(string token)
        {
            if (string.IsNullOrEmpty(token)) token = "default";
            tokens.Remove(token);

            if (log) Debug.Log($"[AudioDucking] End {token}");
        }

        public void ClearAll()
        {
            tokens.Clear();
        }

        public void Pulse(string token, float duckAmount01, float holdSeconds)
        {
            if (holdSeconds <= 0f)
            {
                Begin(token, duckAmount01);
                End(token);
                return;
            }

            StartCoroutine(PulseRoutine(token, duckAmount01, holdSeconds));
        }

        private IEnumerator PulseRoutine(string token, float duckAmount01, float holdSeconds)
        {
            Begin(token, duckAmount01);
            yield return new WaitForSecondsRealtime(holdSeconds);
            End(token);
        }

        private void Apply(float multiplier)
        {
            var sys = AudioSystem.I;
            if (sys != null && IsAlias(target))
            {
                sys.SetDuckMultiplier(target, multiplier);
                return;
            }

            // fallback: direkt VCA (för projekt som vill duck:a en custom VCA)
            if (!directInit) TryInitDirectVca();
            if (!directVca.isValid()) return;

            directVca.setVolume(Mathf.Clamp01(directBase * multiplier));
        }

        private bool IsAlias(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            var a = s.Trim();
            return a.Equals("Master", System.StringComparison.OrdinalIgnoreCase)
                || a.Equals("Music", System.StringComparison.OrdinalIgnoreCase)
                || a.Equals("Sfx", System.StringComparison.OrdinalIgnoreCase)
                || a.Equals("UI", System.StringComparison.OrdinalIgnoreCase);
        }

        private void TryInitDirectVca()
        {
            directInit = true;

            if (string.IsNullOrEmpty(target)) return;
            if (IsAlias(target)) return;

            if (!target.StartsWith("vca:/", System.StringComparison.OrdinalIgnoreCase)) return;

            directVca = RuntimeManager.GetVCA(target);
            if (!directVca.isValid()) return;

            directVca.getVolume(out var cur, out _);
            directBase = Mathf.Clamp01(cur);
        }
    }
}
