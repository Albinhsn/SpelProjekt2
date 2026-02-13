using UnityEngine;

// AudioKit anteckning
// Spelar jingle vid spawn
// Kan triggas från kod
// Bra för portal eller start


namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    public sealed class SpawnJinglePlayer : MonoBehaviour
    {
        [SerializeField] private AudioCueSO jingle;
        [SerializeField] private bool playOnEnable = true;

        private void OnEnable()
        {
            if (!playOnEnable) return;
            if (SfxDirector.I == null) return;
            SfxDirector.I.PlayCue(jingle, transform.position);
        }

        public void Play()
        {
            if (SfxDirector.I == null) return;
            SfxDirector.I.PlayCue(jingle, transform.position);
        }
    }
}
