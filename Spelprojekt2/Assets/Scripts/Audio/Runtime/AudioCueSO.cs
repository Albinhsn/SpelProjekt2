using UnityEngine;
using FMODUnity;

namespace SP2.Audio
{
    // Skapa cues i Projektet och använd dem i prefabs / gameplay.
    [CreateAssetMenu(menuName = "SP2/Audio/Audio Cue", fileName = "AC_")]
    public sealed class AudioCueSO : ScriptableObject
    {
        [Header("== FMOD event ==")]
        public EventReference eventReference;

        [Header("== Standardbeteende ==")]
        public bool is2D = false;

        [Tooltip("Om >0: cue kan inte triggas oftare än cooldown-sek (bra för spam-sfx).")]
        [Min(0f)] public float cooldownSeconds = 0f;

        [Tooltip("Valfritt: ladda sample data vid start (bra för loopar / tunga events).")]
        public bool preloadSampleData = false;
    }
}
