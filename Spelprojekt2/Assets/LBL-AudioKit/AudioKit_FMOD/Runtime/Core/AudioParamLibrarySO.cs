using System;
using System.Collections.Generic;
using UnityEngine;

// AudioKit anteckning
// Lista med "key -> FMOD-namn" för parametrar
// Gör att varje spel kan ha egna param-namn utan att ändra scripts
// Nyckel = det ni skriver i Unity, namn = exakt param-namn i FMOD

namespace AudioKit.FMOD
{
    [CreateAssetMenu(menuName = "AudioKit/Ljud/AudioParamLibrary", fileName = "AudioParamLibrary")]
    public sealed class AudioParamLibrarySO : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            [Tooltip("Valfri nyckel i Unity (t.ex. Indoor, Combat, Speed)")]
            public string key;

            [Tooltip("Exakt param-namn i FMOD")]
            public string fmodName;
        }

        [Tooltip("Lista med key -> FMOD-namn")]
        public Entry[] entries = Array.Empty<Entry>();

        [NonSerialized] private Dictionary<string, string> map;


        public string[] GetKeys()
        {
            if (entries == null) return Array.Empty<string>();
            var list = new List<string>(entries.Length);
            for (int i = 0; i < entries.Length; i++)
            {
                var k = entries[i].key;
                if (string.IsNullOrWhiteSpace(k)) continue;
                list.Add(k.Trim());
            }
            return list.ToArray();
        }

        private void OnValidate()
        {
            map = null;
        }

        public string Resolve(string keyOrName)
        {
            if (string.IsNullOrEmpty(keyOrName)) return null;

            BuildIfNeeded();

            if (map != null && map.TryGetValue(keyOrName, out var name) && !string.IsNullOrEmpty(name))
                return name;

            return keyOrName;
        }

        private void BuildIfNeeded()
        {
            if (map != null) return;

            map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (entries == null) return;

            for (int i = 0; i < entries.Length; i++)
            {
                var e = entries[i];
                if (string.IsNullOrEmpty(e.key)) continue;
                if (string.IsNullOrEmpty(e.fmodName)) continue;

                map[e.key] = e.fmodName;
            }
        }
    }
}
