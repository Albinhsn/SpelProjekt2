using UnityEngine;

// AudioKit anteckning
// Trigger som spelar ett ljud en gång
// Perfekt för pickups och zoner
// Genre-oberoende: filter via tag + layer mask


namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    public sealed class AudioOneShotTrigger : MonoBehaviour
    {
        [Header("Cue")]
        [SerializeField] private AudioCueSO cue;

        [Header("Filter")]
        [SerializeField] private LayerMask activatorLayers = ~0;
        [SerializeField] private string activatorTag = "Player";

        private void Reset()
        {
            var c = GetComponent<Collider>();
            if (c != null) c.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsActivator(other)) return;
            if (SfxDirector.I == null) return;
            SfxDirector.I.PlayCue(cue, transform.position);
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
    }
}
