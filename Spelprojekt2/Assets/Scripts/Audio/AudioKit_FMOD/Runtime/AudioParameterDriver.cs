using System.Collections.Generic;
using UnityEngine;

// AudioKit anteckning
// Samlar zoner för globala parametrar
// Tar högsta prio som är aktiv
// Fade mellan värden


namespace AudioKit.FMOD
{
    public enum KnownGlobalParam
    {
        Combat,
        Indoor,
        DangerZone,
        GameOver
    }

    internal sealed class ParamSource
    {
        public int id;
        public float value;
        public int priority;
        public float fade;
        public bool active;
    }

    internal sealed class ParamState
    {
        public float current;
        public float target;
        public float fade;
    }

    [DisallowMultipleComponent]
    public sealed class AudioParameterDriver : MonoBehaviour
    {
        
        private const float DefaultResetFade = 0.15f;
public static AudioParameterDriver I { get; private set; }

        private AudioConfigSO cfg;

        private readonly Dictionary<string, List<ParamSource>> sources = new Dictionary<string, List<ParamSource>>(8);
        private readonly Dictionary<string, ParamState> states = new Dictionary<string, ParamState>(8);

        private void Awake()
        {
            if (I != null) { Destroy(gameObject); return; }
            I = this;
        }

        private void Start()
        {
            cfg = AudioResources.Config;
        }

        private void Update()
        {
            if (AudioSystem.I == null) return;

            foreach (var kv in sources)
            {
                var paramName = kv.Key;
                var list = kv.Value;

                var best = FindBest(list);

                if (!states.TryGetValue(paramName, out var st))
                {
                    st = new ParamState();
                    st.current = (best != null) ? best.value : 0f;
                    st.target = st.current;
                    st.fade = (best != null) ? best.fade : DefaultResetFade;
                    states[paramName] = st;
                }

                if (best == null)
                {
                    st.target = 0f;
                    st.fade = DefaultResetFade;
                }
                else
                {
                    st.target = best.value;
                    st.fade = best.fade;
                }
if (st.fade <= 0f) st.current = st.target;
                else st.current = Mathf.MoveTowards(st.current, st.target, Time.unscaledDeltaTime / st.fade);

                AudioSystem.I.SetGlobalParam(paramName, st.current);
            }
        }

        private static ParamSource FindBest(List<ParamSource> list)
        {
            ParamSource best = null;

            for (int i = 0; i < list.Count; i++)
            {
                var s = list[i];
                if (!s.active) continue;

                if (best == null) best = s;
                else if (s.priority > best.priority) best = s;
            }

            return best;
        }

        public string Resolve(KnownGlobalParam p)
        {
            if (cfg == null) cfg = AudioResources.Config;
            if (cfg == null) return null;

            if (p == KnownGlobalParam.Combat) return cfg.combatParam;
            if (p == KnownGlobalParam.Indoor) return cfg.indoorParam;
            if (p == KnownGlobalParam.DangerZone) return cfg.dangerZoneParam;
            return cfg.gameOverParam;
        }

        public void SetSourceActive(string paramName, int sourceId, bool active, float value, int priority, float fade)
        {
            if (string.IsNullOrEmpty(paramName)) return;

            if (!sources.TryGetValue(paramName, out var list))
            {
                list = new List<ParamSource>(4);
                sources[paramName] = list;
            }

            var src = FindSource(list, sourceId);
            if (src == null)
            {
                src = new ParamSource();
                src.id = sourceId;
                list.Add(src);
            }

            src.active = active;
            src.value = value;
            src.priority = priority;
            src.fade = fade;
        }

        private static ParamSource FindSource(List<ParamSource> list, int id)
        {
            for (int i = 0; i < list.Count; i++)
                if (list[i].id == id)
                    return list[i];
            return null;
        }

        private void OnDestroy()
        {
            if (I == this) I = null;
        }
    }
}
