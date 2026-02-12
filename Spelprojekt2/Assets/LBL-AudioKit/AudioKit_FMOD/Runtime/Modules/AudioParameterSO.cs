using UnityEngine;

// AudioKit anteckning
// Data-driven referens till en FMOD parameter (global eller event-local)
// Gör att vi slipper hårdkoda parameter-namn i scripts
// Skapa en asset per parameter i varje spel/projekt

namespace AudioKit.FMOD
{
    [CreateAssetMenu(menuName = "AudioKit/Ljud/AudioParameter", fileName = "AP_")]
    public sealed class AudioParameterSO : ScriptableObject
    {
        [Tooltip("FMOD parameter name (exakt stavning).")]
        public string fmodName;

        public bool IsValid => !string.IsNullOrEmpty(fmodName);
    }
}
