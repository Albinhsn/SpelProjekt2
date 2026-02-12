using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

// AudioKit anteckning
// Lista med event IDs till FMOD events
// Ändra här för att undvika ändringar i kod
// Bra för teamarbete


namespace AudioKit.FMOD
{
    [System.Serializable]
    public sealed class AudioEventEntry
    {
        public string id;
        public EventReference evt;

        public bool createInstanceOnStart = true;
        public bool startOnBoot;
        public bool preloadSampleData;
    }

    [CreateAssetMenu(menuName = "AudioKit/Ljud/EventRegistry", fileName = "EventRegistry")]
    public sealed class AudioEventRegistrySO : ScriptableObject
    {
        public AudioEventEntry[] events = System.Array.Empty<AudioEventEntry>();

        private Dictionary<string, EventReference> map;

        private void OnEnable()
        {
            BuildMap();
        }

        private void BuildMap()
        {
            map = new Dictionary<string, EventReference>(events != null ? events.Length : 0);

            if (events == null) return;

            for (int i = 0; i < events.Length; i++)
            {
                var e = events[i];
                if (e == null) continue;
                if (string.IsNullOrWhiteSpace(e.id)) continue;
                if (e.evt.IsNull) continue;

                map[e.id.Trim()] = e.evt;
            }
        }

        public bool TryGetEvent(string id, out EventReference evt)
        {
            evt = default;
            if (string.IsNullOrWhiteSpace(id)) return false;

            if (map == null) BuildMap();

            return map != null && map.TryGetValue(id.Trim(), out evt) && !evt.IsNull;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Uppdatera cache när man ändrar listan i editorn
            BuildMap();
        }
#endif
    }
}
