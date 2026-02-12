using UnityEngine;

// AudioKit anteckning
// Liten driver för intensitet i område
// Kan användas som puzzle-läge
// Start och stop med fade


namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    public sealed class EncounterMusicDriver : MonoBehaviour
    {
        [Header("Stingers")]
        [SerializeField] private AudioCueSO encounterStart;
        [SerializeField] private AudioCueSO encounterEnd;

        [Header("Musik")]
        [SerializeField] private bool quantize = true;
        [Range(0f, 1f)] [SerializeField] private float intensityOnEncounter = 1f;
        [Range(0f, 1f)] [SerializeField] private float dangerOnEncounter = 1f;

        private bool active;

        public void StartEncounter()
        {
            if (active) return;
            active = true;

            if (SfxDirector.I != null && encounterStart != null)
                SfxDirector.I.PlayCue(encounterStart, transform.position);

            ApplyMusic(true);
        }

        public void EndEncounter()
        {
            if (!active) return;
            active = false;

            if (SfxDirector.I != null && encounterEnd != null)
                SfxDirector.I.PlayCue(encounterEnd, transform.position);

            ApplyMusic(false);
        }

        private void ApplyMusic(bool inEncounter)
        {
            if (MusicDirector.I == null) return;

            if (inEncounter)
            {
                if (quantize)
                {
                    MusicDirector.I.SetStateQuantized(MusicState.Encounter);
                    MusicDirector.I.SetIntensityQuantized(intensityOnEncounter);
                    MusicDirector.I.SetDangerQuantized(dangerOnEncounter);
                }
                else
                {
                    MusicDirector.I.SetState(MusicState.Encounter);
                    MusicDirector.I.SetIntensity(intensityOnEncounter);
                    MusicDirector.I.SetDanger(dangerOnEncounter);
                }
            }
            else
            {
                if (quantize) MusicDirector.I.SetStateQuantized(MusicState.Explore);
                else MusicDirector.I.SetState(MusicState.Explore);
            }
        }
    }
}
