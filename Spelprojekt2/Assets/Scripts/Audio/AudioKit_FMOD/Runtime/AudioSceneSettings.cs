using UnityEngine;
using FMODUnity;

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

                if (a.load) RuntimeManager.LoadBank(a.bankName, a.loadSampleData);
                else RuntimeManager.UnloadBank(a.bankName);
            }
        }

        private void ApplyEvents()
        {
            if (eventActions == null) return;

            // Kör actions även om hubben inte är redo ännu.
            // PlayOneShotCue kan hanteras av SfxDirector utan AudioEventHub.
            var pos = GetDefaultPos();

            for (int i = 0; i < eventActions.Length; i++)
                RunAction(eventActions[i], pos);
        }

        private static Vector3 GetDefaultPos()
        {
            var cam = Camera.main;
            if (cam != null) return cam.transform.position;

            var listener = AudioUnityFind.FindAny<AudioListener>(true);
            if (listener != null) return listener.transform.position;

            return Vector3.zero;
        }

        private static void RunAction(AudioEventAction a, Vector3 pos)
        {
            if (a == null) return;

            if (a.actionType == AudioActionType.PlayOneShotCue)
            {
                if (a.cue == null) return;

                // Primärt: SfxDirector (hanterar cooldown + cue.is2D + sampledata)
                if (SfxDirector.I != null)
                {
                    SfxDirector.I.PlayCue(a.cue, pos);
                    return;
                }

                // Fallback: AudioEventHub
                if (AudioEventHub.I != null)
                {
                    var is2D = a.cueIs2D || a.cue.is2D;
                    AudioEventHub.I.PlayOneShot(a.cue.evt, pos, is2D);
                }

                return;
            }

            // Resten kräver hub
            if (AudioEventHub.I == null) return;

            if (a.actionType == AudioActionType.PlayEvent)
                AudioEventHub.I.Play(a.eventId);

            else if (a.actionType == AudioActionType.StopEvent)
                AudioEventHub.I.Stop(a.eventId, a.allowFadeout);

            else if (a.actionType == AudioActionType.SetEventParameter)
            {
                var pName = (a.parameter != null && a.parameter.IsValid) ? a.parameter.fmodName : a.parameterName;
                AudioEventHub.I.SetEventParam(a.eventId, pName, a.parameterValue);
            }

            else if (a.actionType == AudioActionType.SetGlobalParameter)
            {
                var pName = (a.parameter != null && a.parameter.IsValid) ? a.parameter.fmodName : a.parameterName;
                AudioEventHub.I.SetGlobalParam(pName, a.parameterValue);
            }
        }
    }
}
