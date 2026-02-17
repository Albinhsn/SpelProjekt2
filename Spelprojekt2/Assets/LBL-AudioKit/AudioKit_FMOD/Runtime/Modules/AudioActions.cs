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

        public void Run(Vector3 pos)
        {
            var sys = AudioSystem.I;
            var hub = AudioEventHub.I;

            AudioAction a = this;
            switch (a.type)
            {
                case AudioActionType.SetVca:
                    if (sys != null) sys.SetVca(a.vcaName, a.vcaValue);
                    break;

                case AudioActionType.PlayEvent:
                    if (hub != null) hub.PlayOneShot(a.eventId, pos);
                    break;

                case AudioActionType.StartLoop:
                    if (hub != null) hub.StartLoop(a.eventId);
                    break;

                case AudioActionType.StopLoop:
                    if (hub != null) hub.Stop(a.eventId, a.fade);
                    break;

                case AudioActionType.SetGlobalParam:
                {
                    if (sys == null) break;
                    var pName = (a.parameter != null && a.parameter.IsValid) ? a.parameter.fmodName : a.parameterName;
                    pName = AudioParamUtil.Resolve(pName);
                    if (string.IsNullOrEmpty(pName)) break;
                    sys.SetGlobalParam(pName, a.parameterValue);
                    break;
                }

                case AudioActionType.SetEventParameter:
                {
                    if (hub == null) break;
                    var pName = (a.parameter != null && a.parameter.IsValid) ? a.parameter.fmodName : a.parameterName;
                    pName = AudioParamUtil.Resolve(pName);
                    if (string.IsNullOrEmpty(pName)) break;
                    hub.SetEventParam(a.eventId, pName, a.parameterValue);
                    break;
                }

                case AudioActionType.SetSnapshot:
                    if (sys != null) sys.SetSnapshot(a.snapshot, a.snapshotEnable, a.fade);
                    break;
            }

        }
    }
}
