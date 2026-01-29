using UnityEngine;
using FMODUnity;

namespace SP2.Audio
{
    // Placera asset i: Assets/Resources/Audio/AudioConfig.asset
    [CreateAssetMenu(menuName = "SP2/Audio/Audio Config", fileName = "AudioConfig")]
    public sealed class AudioConfigSO : ScriptableObject
    {
        [Header("== Music (persistent) ==")]
        public EventReference mainMusicEvent;

        [Header("== Music parameters (event-local) ==")]
        public string musicStateParam = "MusicState";
        public string intensityParam = "Intensity";
        public string dangerParam = "Danger";

        [Header("== Start (musik) ==")]
        public MusicState startMusicState = MusicState.Menu;
        [UnityEngine.Range(0f, 1f)] public float startIntensity = 0f;

        [Header("== Quantize (musik) ==")]
        public bool quantizeOnBar = true;

        [Header("== Known global parameters ==")]
        public string combatParam = "Combat";
        public string indoorParam = "Indoor";
        public string dangerZoneParam = "DangerZone";
        public string gameOverParam = "GameOver";

        [Header("== Snapshots ==")]
        public EventReference pauseSnapshot;

        [Header("== Mixer paths ==")]
        public string busMaster = "bus:/";
        public string vcaMusic = "vca:/Music";
        public string vcaSfx = "vca:/SFX";
        public string vcaUi = "vca:/UI";

        [Header("== Default volumes (0..1) ==")]
        [Range(0f, 1f)] public float defaultMaster = 1f;
        [Range(0f, 1f)] public float defaultMusic = 1f;
        [Range(0f, 1f)] public float defaultSfx = 1f;
        [Range(0f, 1f)] public float defaultUi = 1f;

        [Header("== PlayerPrefs keys ==")]
        public string prefMaster = "SP2_VOL_MASTER";
        public string prefMusic = "SP2_VOL_MUSIC";
        public string prefSfx = "SP2_VOL_SFX";
        public string prefUi = "SP2_VOL_UI";

        [Header("== Listener (FMOD StudioListener) ==")]
        public bool ensureStudioListener = true;
    }
}
