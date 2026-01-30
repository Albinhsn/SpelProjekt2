using UnityEngine;
using FMODUnity;

// AudioKit anteckning
// Central inställning för FMOD
// Här sätter du paths och param-namn
// Delas mellan scener


namespace AudioKit.FMOD
{
    [CreateAssetMenu(menuName = "AudioKit/Ljud/AudioConfig", fileName = "AudioConfig")]
    public sealed class AudioConfigSO : ScriptableObject
    {
        [Header("Musik")]
        public EventReference mainMusicEvent;
        public string mainMusicPath = "event:/Music/Main";

        [Header("Musikparametrar i eventet")]
        public string musicStateParam = "MusicState";
        public string intensityParam = "Intensity";
        public string dangerParam = "Danger";

        [Header("Globala parametrar")]
        public string combatParam = "Combat";
        public string indoorParam = "Indoor";
        public string dangerZoneParam = "DangerZone";
        public string gameOverParam = "GameOver";

        [Header("VCA")]
        public string vcaMaster = "vca:/Master";
        public string vcaMusic = "vca:/Music";
        public string vcaSfx = "vca:/SFX";
        public string vcaUi = "vca:/UI";

        [Header("Bus")]
        public string masterBus = "bus:/";

        [Header("Snapshots")]
        public EventReference pauseSnapshot;
        public string pauseSnapshotPath = "snapshot:/Pause";
        public EventReference damageSnapshot;
        public string damageSnapshotPath = "snapshot:/Damage";

        [Header("Standardvolym")]
        [Range(0f, 1f)] public float masterDefault = 1f;
        [Range(0f, 1f)] public float musicDefault = 1f;
        [Range(0f, 1f)] public float sfxDefault = 1f;
        [Range(0f, 1f)] public float uiDefault = 1f;

        [Header("Lyssnare")]
        public bool ensureStudioListener = true;
    }
}
