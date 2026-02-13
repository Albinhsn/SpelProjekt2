using UnityEngine;

// AudioKit anteckning
// Kör en lista med audio-actions när scenen startar
// Bra för att sätta startvolymer, snapshots och default parametrar

namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    [AddComponentMenu("AudioKit/Setup/Audio Scene Settings")]
    public sealed class AudioSceneSettings : MonoBehaviour
    {
        [SerializeField] private bool runOnStart = true;
        [SerializeField] private AudioAction[] actions = new AudioAction[0];

        private void Start()
        {
            if (runOnStart) ApplyActions();
        }

        [ContextMenu("Kör Audio Actions")]
        public void ApplyActions()
        {
            if (actions == null || actions.Length == 0) return;

            var sys = AudioSystem.I;
            var hub = AudioEventHub.I;

            for (int i = 0; i < actions.Length; i++)
            {
                var a = actions[i];

                switch (a.type)
                {
                    case AudioActionType.SetVca:
                        sys?.SetVca(a.vcaName, a.vcaValue);
                        break;

                    case AudioActionType.PlayEvent:
                        hub?.PlayOneShot(a.eventId, transform.position);
                        break;

                    case AudioActionType.StartLoop:
                        hub?.PlayIfNotPlaying(a.eventId);
                        break;

                    case AudioActionType.StopLoop:
                        hub?.Stop(a.eventId, a.fade);
                        break;

                    case AudioActionType.SetGlobalParam:
                    {
                        var pName = (a.parameter != null && a.parameter.IsValid) ? a.parameter.fmodName : a.parameterName;
                        pName = AudioParamUtil.Resolve(pName);
                        if (string.IsNullOrEmpty(pName)) break;
                        sys?.SetGlobalParam(pName, a.parameterValue);
                        break;
                    }

                    case AudioActionType.SetEventParameter:
                    {
                        var pName = (a.parameter != null && a.parameter.IsValid) ? a.parameter.fmodName : a.parameterName;
                        pName = AudioParamUtil.Resolve(pName);
                        if (string.IsNullOrEmpty(pName)) break;
                        hub?.SetEventParam(a.eventId, pName, a.parameterValue);
                        break;
                    }

                    case AudioActionType.SetSnapshot:
                        sys?.SetSnapshot(a.snapshot, a.snapshotEnable, a.fade);
                        break;
                }
            }
        }
    }
}
