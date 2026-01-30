using UnityEngine;
using UnityEngine.Events;

// AudioKit anteckning
// Trigger som kör en AudioActions-lista
// Bra för zoner, portar och cutscenes
// Kräver Player-tag


namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class AudioTrigger : MonoBehaviour
    {
        [Header("Filter")]
        [SerializeField] private LayerMask activatorLayers = ~0;
        [SerializeField] private string activatorTag = "Player";

        [Header("Actions")]
        [SerializeField] private bool runOnEnter = true;
        [SerializeField] private bool runOnExit;
        [SerializeField] private AudioEventAction[] actions = System.Array.Empty<AudioEventAction>();

        [Header("UnityEvent")]
        [SerializeField] private UnityEvent onTriggered;

        private void Reset()
        {
            var c = GetComponent<Collider>();
            if (c != null) c.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!runOnEnter) return;
            if (!IsActivator(other)) return;
            Run();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!runOnExit) return;
            if (!IsActivator(other)) return;
            Run();
        }

        private bool IsActivator(Collider other)
        {
            if (!string.IsNullOrEmpty(activatorTag) && !other.CompareTag(activatorTag))
                return false;

            if (activatorLayers != (activatorLayers | (1 << other.gameObject.layer)))
                return false;

            return true;
        }

        public void Run()
        {
            if (actions != null)
            {
                for (int i = 0; i < actions.Length; i++)
                    AudioSceneSettings_RunAction(actions[i]);
            }

            onTriggered?.Invoke();
        }

        private static void AudioSceneSettings_RunAction(AudioEventAction a)
        {
            if (a == null) return;

            if (a.actionType == AudioActionType.PlayOneShotCue)
            {
                if (SfxDirector.I != null && a.cue != null)
                    SfxDirector.I.PlayCue(a.cue, Vector3.zero);

                else if (AudioEventHub.I != null && a.cue != null)
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
