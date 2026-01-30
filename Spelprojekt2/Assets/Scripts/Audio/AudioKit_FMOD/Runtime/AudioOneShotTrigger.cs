using UnityEngine;

// AudioKit anteckning
// Trigger som spelar ett ljud en gång
// Perfekt för pickups och zoner
// Kräver Player-tag


namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    public sealed class AudioOneShotTrigger : MonoBehaviour
    {
        [SerializeField] private AudioCueSO cue;

        private void Reset()
        {
            var c = GetComponent<Collider>();
            if (c != null) c.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (SfxDirector.I == null) return;
            SfxDirector.I.PlayCue(cue, transform.position);
        }
    }
}
