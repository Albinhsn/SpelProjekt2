using UnityEngine;
using FMODUnity;

// AudioKit anteckning
// Lista med event IDs till FMOD paths
// Ändra här så slipper du ändra i kod
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
    }
}
