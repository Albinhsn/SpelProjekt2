using UnityEngine;
using FMODUnity;

// AudioKit anteckning
// Projektets centrala AudioConfig.
// Håll den *generisk* så att AudioKit kan återanvändas mellan spel.
// Parametrar ska normalt inte hårdkodas här – använd AudioParamLibrary + AudioParamRef på respektive komponent.

namespace AudioKit.FMOD
{
    [CreateAssetMenu(menuName = "AudioKit/Ljud/AudioConfig", fileName = "AudioConfig")]
    public sealed class AudioConfigSO : ScriptableObject
    {
        [Header("Main Music (valfritt)")]
        public EventReference mainMusicEvent;

        [Tooltip("Fallback om du inte vill dra EventReference. Ex: 'event:/Music/Main'")]
        public string mainMusicPath = "";

        [Header("VCA (valfritt)")]
        [Tooltip("Standard: vca:/Master")]
        public string vcaMaster = "vca:/Master";

        [Tooltip("Standard: vca:/Music")]
        public string vcaMusic = "vca:/Music";

        [Tooltip("Standard: vca:/SFX")]
        public string vcaSfx = "vca:/SFX";

        [Tooltip("Standard: vca:/UI")]
        public string vcaUi = "vca:/UI";

        [Header("Bus (valfritt)")]
        [Tooltip("Fallback om Master VCA saknas. Standard: bus:/")]
        public string masterBus = "bus:/";

        [Header("Snapshots (valfritt)")]
        public EventReference pauseSnapshot;
        public string pauseSnapshotPath = "";

        public EventReference damageSnapshot;
        public string damageSnapshotPath = "";

        [Header("Standardvolym")]
        [Range(0f, 1f)] public float masterDefault = 1f;
        [Range(0f, 1f)] public float musicDefault = 1f;
        [Range(0f, 1f)] public float sfxDefault = 1f;
        [Range(0f, 1f)] public float uiDefault = 1f;

        [Header("Lyssnare")]
        public bool ensureStudioListener = true;

        // ---
        // OBS:
        // Tidigare fanns hårdkodade param-fält här (Combat/Indoor/GameOver, MusicState/Intensity/Danger, osv).
        // De togs bort för att AudioKit ska fungera i flera spel med olika parameternamn och olika antal parametrar.
        // Använd istället:
        // - AudioParamLibrarySO (key -> FMOD-namn)
        // - AudioParamRef på komponenterna som behöver en parameter
        // ---
    }
}
