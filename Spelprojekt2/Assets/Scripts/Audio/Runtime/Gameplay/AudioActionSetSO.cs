using System;
using UnityEngine;

namespace SP2.Audio
{
    public enum AudioVolumeTarget
    {
        Master,
        Music,
        Sfx,
        Ui
    }

    public enum AudioActionType
    {
        PlayCue,

        SetMusicState,
        SetMusicIntensity,
        SetMusicDanger,

        SetGlobalParamRequest,
        ClearGlobalParamRequest,

        SetPauseSnapshot,
        SetVolume
    }

    [Serializable]
    public struct AudioAction
    {
        [Header("== Action ==")]
        public AudioActionType type;

        [Header("== SFX ==")]
        public AudioCueSO cue;
        public bool force2D;

        [Header("== Music ==")]
        public MusicState musicState;
        [Range(0f, 1f)] public float value01;

        [Header("== Global Parameter ==")]
        public KnownGlobalParam knownParam;
        public string customParamName;
        [Range(0f, 1f)] public float globalValue01;
        public int priority;
        [Min(0f)] public float fadeSeconds;
        public bool useUnscaledTime;

        [Header("== Mixer ==")]
        public AudioVolumeTarget volumeTarget;
        [Range(0f, 1f)] public float volume01;
        public bool boolValue;

        public string ResolveGlobalParamName(AudioConfigSO cfg)
        {
            if (knownParam == KnownGlobalParam.Custom)
                return customParamName;

            if (cfg == null)
                return customParamName;

            return knownParam switch
            {
                KnownGlobalParam.Combat => cfg.combatParam,
                KnownGlobalParam.Indoor => cfg.indoorParam,
                KnownGlobalParam.DangerZone => cfg.dangerZoneParam,
                KnownGlobalParam.GameOver => cfg.gameOverParam,
                _ => customParamName
            };
        }
    }

    // Example: "BossStart", "SceneStart", "CheckpointReached"
    [CreateAssetMenu(menuName = "SP2/Audio/Audio Action Set", fileName = "AAS_")]
    public sealed class AudioActionSetSO : ScriptableObject
    {
        public AudioAction[] actions = Array.Empty<AudioAction>();
    }
}
