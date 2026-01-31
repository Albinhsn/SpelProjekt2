using UnityEngine;
using UnityEngine.Events;

// Trigger som kör en AudioActions-lista
// Bra för zoner, portar och cutscenes
// Kräver Player-tag (eller tag på root/rigidbody)

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
            if (other == null) return false;

            if (!string.IsNullOrEmpty(activatorTag) && !HasTag(other, activatorTag))
                return false;

            if (activatorLayers != (activatorLayers | (1 << other.gameObject.layer)))
                return false;

            return true;
        }

        private static bool HasTag(Collider other, string tag)
        {
            if (other.CompareTag(tag)) return true;

            var rb = other.attachedRigidbody;
            if (rb != null && rb.CompareTag(tag)) return true;

            var root = other.transform.root;
            if (root != null && root.CompareTag(tag)) return true;

            return false;
        }

        public void Run()
        {
            var pos = transform.position;

            if (actions != null)
            {
                for (int i = 0; i < actions.Length; i++)
                    RunAction(actions[i], pos);
            }

            onTriggered?.Invoke();
        }

        private static void RunAction(AudioEventAction a, Vector3 pos)
        {
            if (a == null) return;

            if (a.actionType == AudioActionType.PlayOneShotCue)
            {
                if (a.cue == null) return;

                // SfxDirector använder cue.is2D automatiskt
                if (SfxDirector.I != null)
                {
                    SfxDirector.I.PlayCue(a.cue, pos);
                    return;
                }

                // Fallback: AudioEventHub one-shot
                if (AudioEventHub.I != null)
                {
                    var is2D = a.cueIs2D || a.cue.is2D;
                    AudioEventHub.I.PlayOneShot(a.cue.evt, pos, is2D);
                }

                return;
            }

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
