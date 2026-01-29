using System.Collections.Generic;
using UnityEngine;

namespace SP2.Audio
{
    // Zoner/logic pushar "requests" -> driver väljer högsta prio.
    [DefaultExecutionOrder(-60)]
    public sealed class AudioParameterDriver : MonoBehaviour
    {
        private struct Request
        {
            public Object owner;
            public float value;
            public int priority;
            public float fadeSeconds;
            public bool useUnscaled;
        }

        private struct State
        {
            public float current;
            public float target;
            public float fadeSeconds;
            public bool useUnscaled;
            public List<Request> requests;
        }

        private readonly Dictionary<string, State> _state = new();
        private readonly List<string> _keysBuffer = new();

        public void SetRequest(Object owner, string paramName, float value, int priority, float fadeSeconds, bool useUnscaledTime)
        {
            if (owner == null) return;
            if (string.IsNullOrEmpty(paramName)) return;

            if (!_state.TryGetValue(paramName, out State st))
            {
                st = new State { requests = new List<Request>(4), current = value, target = value, fadeSeconds = fadeSeconds, useUnscaled = useUnscaledTime };
            }

            // update or add
            bool updated = false;
            for (int i = 0; i < st.requests.Count; i++)
            {
                if (st.requests[i].owner == owner)
                {
                    var r = st.requests[i];
                    r.value = value;
                    r.priority = priority;
                    r.fadeSeconds = fadeSeconds;
                    r.useUnscaled = useUnscaledTime;
                    st.requests[i] = r;
                    updated = true;
                    break;
                }
            }

            if (!updated)
            {
                st.requests.Add(new Request
                {
                    owner = owner,
                    value = value,
                    priority = priority,
                    fadeSeconds = fadeSeconds,
                    useUnscaled = useUnscaledTime
                });
            }

            _state[paramName] = st;
        }

        public void ClearRequest(Object owner, string paramName)
        {
            if (owner == null) return;
            if (string.IsNullOrEmpty(paramName)) return;

            if (!_state.TryGetValue(paramName, out State st))
                return;

            for (int i = st.requests.Count - 1; i >= 0; i--)
            {
                if (st.requests[i].owner == owner)
                    st.requests.RemoveAt(i);
            }

            _state[paramName] = st;
        }

        private void Update()
        {
            if (_state.Count == 0) return;

            // cleanup + apply
            _keysBuffer.Clear();
            foreach (var kv in _state) _keysBuffer.Add(kv.Key);

            for (int k = 0; k < _keysBuffer.Count; k++)
            {
                string key = _keysBuffer[k];
                var st = _state[key];
                if (st.requests == null)
                    continue;

                // remove destroyed owners
                for (int i = st.requests.Count - 1; i >= 0; i--)
                {
                    if (st.requests[i].owner == null)
                        st.requests.RemoveAt(i);
                }

                // target = best request or 0
                float target = 0f;
                int bestPrio = int.MinValue;
                float bestFade = 0f;
                bool bestUnscaled = false;

                for (int i = 0; i < st.requests.Count; i++)
                {
                    var r = st.requests[i];
                    if (r.priority > bestPrio)
                    {
                        bestPrio = r.priority;
                        target = r.value;
                        bestFade = r.fadeSeconds;
                        bestUnscaled = r.useUnscaled;
                    }
                }

                st.target = target;
                st.fadeSeconds = bestFade;
                st.useUnscaled = bestUnscaled;

                float dt = st.useUnscaled ? Time.unscaledDeltaTime : Time.deltaTime;

                if (st.fadeSeconds <= 0f)
                {
                    st.current = st.target;
                }
                else
                {
                    float maxDelta = dt / st.fadeSeconds;
                    st.current = Mathf.MoveTowards(st.current, st.target, maxDelta);
                }

                FmodSafe.SetGlobalParameterSafe(key, st.current);

                // remove empty to avoid growth
                if (st.requests.Count == 0 && Mathf.Approximately(st.current, 0f))
                {
                    _state.Remove(key);
                    continue;
                }

                _state[key] = st;
            }
        }
    }
}
