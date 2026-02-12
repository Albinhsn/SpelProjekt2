using UnityEngine;

// AudioKit anteckning
// Styr encounter music instance
// Sätter parametrar och fade
// Används av driver och zoner


namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    public sealed class EncounterMusicController : MonoBehaviour
    {
        [Header("Koppling")]
        [SerializeField] private EncounterMusicDriver driver;

        [Header("Auto")]
        [SerializeField] private bool startOnEnable = true;

        private void Reset()
        {
            driver = GetComponent<EncounterMusicDriver>();
        }

        private void OnEnable()
        {
            if (!startOnEnable) return;
            if (driver == null) driver = GetComponent<EncounterMusicDriver>();
            if (driver != null) driver.StartEncounter();
        }

        public void StartEncounter()
        {
            if (driver == null) driver = GetComponent<EncounterMusicDriver>();
            if (driver != null) driver.StartEncounter();
        }

        public void EndEncounter()
        {
            if (driver == null) driver = GetComponent<EncounterMusicDriver>();
            if (driver != null) driver.EndEncounter();
        }
    }
}
