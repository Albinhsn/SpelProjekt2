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
        [SerializeField] private AudioAction[] startActions = new AudioAction[0];
        [SerializeField] private AudioAction[] endActions = new AudioAction[0];

        private void Start()
        {
            if (runOnStart) ApplyStartActions();
        }

        [ContextMenu("Kör Audio Actions")]
        private void ApplyActions(AudioAction[] actions)
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
                        if (sys == null) break;

                        var pName = (a.parameter != null && a.parameter.IsValid) ? a.parameter.fmodName : a.parameterName;
                        pName = AudioParamUtil.Resolve(pName);
                        if (!string.IsNullOrEmpty(pName))
                        {
                            sys.SetGlobalParam(pName, a.parameterValue);
                        } 
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

        public void ApplyStartActions()
        {
            ApplyActions(startActions);
        }

        public void ApplyEndActions()
        {
            ApplyActions(endActions);
        }
    }
}
