using System;
using UnityEngine;

// AudioKit anteckning
// Delade actions som kan köras av AudioTrigger / AudioSceneSettings
// Enkelt sätt att testa eller bygga små kedjor utan ny kod

namespace AudioKit.FMOD
{
    public enum AudioActionType
    {
        SetVca,
        PlayEvent,
        StartLoop,
        StopLoop,
        SetGlobalParam,
        SetEventParameter,
        SetSnapshot
    }

    [Serializable]
    public struct AudioAction
    {
        public AudioActionType type;

        [Header("Event")]
        public string eventId;

        [Header("VCA")]
        public string vcaName;
        public float vcaValue;

        [Header("Parameter")]
        public AudioParameterSO parameter;
        public string parameterName;
        public float parameterValue;

        [Header("Snapshot")]
        public string snapshot;
        public bool snapshotEnable;

        [Header("Fade")]
        public float fade;
    }
}
