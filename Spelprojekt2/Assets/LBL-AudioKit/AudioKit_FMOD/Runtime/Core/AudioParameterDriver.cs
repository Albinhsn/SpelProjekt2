using System.Collections.Generic;
using UnityEngine;

// AudioKit anteckning
// Driver globala parametrar från flera källor (zoner, states, triggers)
// Högst prioritet vinner per parameter
// Param-namn kan vara key (via AudioParamLibrary) eller direkt FMOD-namn

namespace AudioKit.FMOD
{
    // Behålls främst för bakåtkompatibilitet med äldre scener.
    // Rekommenderat: använd AudioParamRef (asset/key/namn) direkt i komponenterna.
    public enum KnownGlobalParam
    {
        Combat,
        Indoor,
        DangerZone,
        GameOver,
        Custom
    }

    [DisallowMultipleComponent]
    [AddComponentMenu("AudioKit/Core/Audio Parameter Driver")]
    public sealed class AudioParameterDriver : MonoBehaviour
    {
        public static AudioParameterDriver I { get; private set; }

        private sealed class Source
        {
            public string param;
            public int priority;
            public float target;
            public float fade;
            public bool active;
        }

        private struct ParamRuntime
        {
            public float current;
        }

        private struct Best
        {
            public int priority;
            public float target;
            public float fade;
            public bool any;
        }

        // key = paramName|sourceId
        private readonly Dictionary<string, Source> sources = new Dictionary<string, Source>(64);

        // persistent current values
        private readonly Dictionary<string, ParamRuntime> runtime = new Dictionary<string, ParamRuntime>(32);

        // per frame best target
        private readonly Dictionary<string, Best> best = new Dictionary<string, Best>(32);

        [Header("Debug")]
        [SerializeField] private bool logActive;

        private void Awake()
        {
            if (I != null && I != this)
            {
                Destroy(gameObject);
                return;
            }

            I = this;
        }

        private void OnDestroy()
        {
            if (I == this) I = null;
        }


        // Resolve känd parameter till faktiska FMOD-namnet via AudioParamLibrary.
        // Keys som används:
        // - Combat, Indoor, DangerZone, GameOver (enum-namnen)
        // Om library saknas används key som name.
        public string Resolve(KnownGlobalParam p, string custom = null)
        {
            var lib = AudioResources.ParamLibrary;

            if (p == KnownGlobalParam.Custom)
                return AudioParamUtil.Resolve(lib != null ? lib.Resolve(custom) : custom);

            var key = p.ToString();
            var raw = lib != null ? lib.Resolve(key) : key;
            return AudioParamUtil.Resolve(raw);
        }

        private void Update()
        {
            if (AudioSystem.I == null) return;

            best.Clear();

            foreach (var kv in sources)
            {
                var s = kv.Value;
                if (s == null || !s.active) continue;
                if (string.IsNullOrEmpty(s.param)) continue;

                if (!best.TryGetValue(s.param, out var b) || !b.any || s.priority > b.priority)
                {
                    b.any = true;
                    b.priority = s.priority;
                    b.target = s.target;
                    b.fade = s.fade;
                    best[s.param] = b;
                }
            }

            foreach (var kv in best)
            {
                var param = kv.Key;
                var b = kv.Value;
                if (!b.any) continue;

                if (!runtime.TryGetValue(param, out var r))
                    r = new ParamRuntime { current = 0f };

                if (b.fade <= 0f)
                    r.current = b.target;
                else
                    r.current = Mathf.MoveTowards(r.current, b.target, Time.deltaTime / b.fade);

                runtime[param] = r;
                AudioSystem.I.SetGlobalParam(param, r.current);

                if (logActive)
                    Debug.Log($"[AudioParameterDriver] {param} = {r.current:0.00} -> {b.target:0.00} (prio {b.priority})");
            }
        }

        public void SetSourceActive(string sourceId, KnownGlobalParam known, string customParam, float value, bool active, int priority, float fade)
        {
            var name = Resolve(known, customParam);
            if (string.IsNullOrEmpty(name)) return;

            SetSourceActiveInternal(sourceId, name, value, active, priority, fade);
        }

        public void SetSourceActive(string sourceId, string paramNameOrKey, float value, bool active, int priority, float fade)
        {
            var name = AudioParamUtil.Resolve(paramNameOrKey);
            if (string.IsNullOrEmpty(name)) return;

            SetSourceActiveInternal(sourceId, name, value, active, priority, fade);
        }

        private void SetSourceActiveInternal(string sourceId, string paramName, float value, bool active, int priority, float fade)
        {
            if (string.IsNullOrEmpty(sourceId)) return;
            if (string.IsNullOrEmpty(paramName)) return;

            var key = paramName + "|" + sourceId;

            if (!sources.TryGetValue(key, out var s) || s == null)
            {
                s = new Source();
                sources[key] = s;
            }

            s.param = paramName;
            s.priority = priority;
            s.target = value;
            s.fade = Mathf.Max(0f, fade);
            s.active = active;
        }

        // ResolveRaw togs bort (AudioConfig innehåller inte längre hårdkodade param-fält).


        // --- Debug / verktygs-API (t.ex. för overlay) ---
        public int ActiveParamCount => runtime != null ? runtime.Count : 0;

        public void GetActiveParams(List<string> outParams)
        {
            if (outParams == null) return;
            outParams.Clear();
            if (runtime == null) return;

            foreach (var kv in runtime)
                outParams.Add(kv.Key);
        }

        public bool TryGetCurrentValue(string paramKeyOrName, out float value)
        {
            value = 0f;
            var name = AudioParamUtil.Resolve(paramKeyOrName);
            if (string.IsNullOrEmpty(name)) return false;

            if (runtime != null && runtime.TryGetValue(name, out var r))
            {
                value = r.current;
                return true;
            }
            return false;
        }
    }
}
