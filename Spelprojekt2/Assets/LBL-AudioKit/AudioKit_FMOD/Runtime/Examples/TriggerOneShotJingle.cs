using UnityEngine;

// AudioKit anteckning
// Trigger som spelar en jingle
// För portal, checkpoint eller unlock
// Kräver Player-tag


namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class TriggerOneShotJingle : MonoBehaviour
    {
        [SerializeField] private AudioCueSO cue;
        [SerializeField] private string activatorTag = "Player";

        private void Reset()
        {
            var c = GetComponent<Collider>();
            if (c != null) c.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!string.IsNullOrEmpty(activatorTag) && !other.CompareTag(activatorTag))
                return;

            if (SfxDirector.I != null)
                SfxDirector.I.PlayCue(cue, transform.position);
        }
    }
}
