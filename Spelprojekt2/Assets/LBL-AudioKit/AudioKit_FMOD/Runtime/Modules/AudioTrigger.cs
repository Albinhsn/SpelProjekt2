using UnityEngine;
using UnityEngine.Events;

// AudioKit anteckning
// Trigger som kör en lista av AudioAction (samma som AudioSceneSettings)
// Bra för zoner, portar och cutscenes.
// Filter: tag + layer mask. Tag kan ligga på collider, rigidbody eller root.

namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("AudioKit/Triggers/Audio Trigger")]
    public sealed class AudioTrigger : MonoBehaviour
    {
        [Header("Filter")]
        [SerializeField] private LayerMask activatorLayers = ~0;
        [SerializeField] private string activatorTag = "Player";

        [Header("Run")]
        [SerializeField] private bool runOnEnter = true;
        [SerializeField] private bool runOnExit = false;

        [Header("Actions")]
        [SerializeField] private AudioAction[] actions = System.Array.Empty<AudioAction>();

        [Header("UnityEvent (optional)")]
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

            if (!string.IsNullOrEmpty(activatorTag) && !AudioActivatorUtil.HasTag(other, activatorTag))
                return false;

            var layer = other.gameObject.layer;
            if ((activatorLayers.value & (1 << layer)) == 0)
                return false;

            return true;
        }

        [ContextMenu("Run Actions")]
        public void Run()
        {
            var pos = transform.position;

            if (actions != null && actions.Length > 0)
            {
                for (int i = 0; i < actions.Length; i++)
                    RunAction(actions[i], pos);
            }

            onTriggered?.Invoke();
        }

        private static void RunAction(AudioAction a, Vector3 pos)
        {
            // Om systemet inte är bootat än, gör inget (undvik NRE).
            var hub = AudioEventHub.I;
            var sys = AudioSystem.I;

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
