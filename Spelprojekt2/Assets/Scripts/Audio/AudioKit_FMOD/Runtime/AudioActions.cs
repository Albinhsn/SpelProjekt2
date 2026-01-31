using UnityEngine;

// AudioKit anteckning
// En samling audio-kommandon
// Spela cue, sätt param, stoppa musik
// Används av AudioTrigger och SceneSettings


namespace AudioKit.FMOD
{
    public enum AudioActionType
    {
        PlayEvent,
        StopEvent,
        SetEventParameter,
        SetGlobalParameter,
        PlayOneShotCue
    }

    [System.Serializable]
    public sealed class AudioEventAction
    {
        [Header("Event")]
        public string eventId;

        public AudioActionType actionType = AudioActionType.PlayEvent;

        [Header("Parameter")]
        public string parameterName;

        
        [Tooltip("Valfritt: om satt används denna istället för parameterName.")]
        public AudioParameterSO parameter;
public float parameterValue;

        [Header("Stop")]
        public bool allowFadeout = true;

        [Header("One shot")]
        public AudioCueSO cue;
        public bool cueIs2D;
    }

    [System.Serializable]
    public sealed class AudioBankAction
    {
        public string bankName;
        public bool load = true;
        public bool loadSampleData;
    }
}
