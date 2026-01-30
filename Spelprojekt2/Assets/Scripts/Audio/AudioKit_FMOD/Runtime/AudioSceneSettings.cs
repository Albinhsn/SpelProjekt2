using UnityEngine;
using FMODUnity;

// AudioKit anteckning
// Sätter audio-läge när scenen laddar
// Bra för hub och världar
// Kan även återställa vid exit


namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    public sealed class AudioSceneSettings : MonoBehaviour
    {
        [Header("Körning")]
        [SerializeField] private bool runOnStart = true;
        [SerializeField] private bool runOnEnable;
        [SerializeField] private bool runOnce = true;

        [Header("Actions")]
        [SerializeField] private AudioEventAction[] eventActions = System.Array.Empty<AudioEventAction>();
        [SerializeField] private AudioBankAction[] bankActions = System.Array.Empty<AudioBankAction>();

        private bool hasRun;

        private void OnEnable()
        {
            if (runOnEnable) Apply();
        }

        private void Start()
        {
            if (runOnStart) Apply();
        }

        public void Apply()
        {
            if (runOnce && hasRun) return;
            hasRun = true;

            ApplyBanks();
            ApplyEvents();
        }

        private void ApplyBanks()
        {
            if (bankActions == null) return;

            for (int i = 0; i < bankActions.Length; i++)
            {
                var a = bankActions[i];
                if (a == null) continue;
                if (string.IsNullOrEmpty(a.bankName)) continue;

                if (a.load) FMODUnity.RuntimeManager.LoadBank(a.bankName, a.loadSampleData);
                else FMODUnity.RuntimeManager.UnloadBank(a.bankName);
            }
        }

        private void ApplyEvents()
        {
            if (eventActions == null) return;
            if (AudioEventHub.I == null) return;

            for (int i = 0; i < eventActions.Length; i++)
                RunAction(eventActions[i]);
        }

        private static void RunAction(AudioEventAction a)
        {
            if (a == null) return;

            if (a.actionType == AudioActionType.PlayOneShotCue)
            {
                if (SfxDirector.I != null && a.cue != null)
                {
                    SfxDirector.I.PlayCue(a.cue, Vector3.zero);
                    return;
                }

                if (AudioEventHub.I != null && a.cue != null)
                    AudioEventHub.I.PlayOneShot(a.cue.evt, Vector3.zero, a.cueIs2D);

                return;
            }

            if (AudioEventHub.I == null) return;

            if (a.actionType == AudioActionType.PlayEvent)
                AudioEventHub.I.Play(a.eventId);

            else if (a.actionType == AudioActionType.StopEvent)
                AudioEventHub.I.Stop(a.eventId, a.allowFadeout);

            else if (a.actionType == AudioActionType.SetEventParameter)
                AudioEventHub.I.SetEventParam(a.eventId, a.parameterName, a.parameterValue);

            else if (a.actionType == AudioActionType.SetGlobalParameter)
                AudioEventHub.I.SetGlobalParam(a.parameterName, a.parameterValue);
        }
    }
}
