using UnityEngine;

// AudioKit anteckning
// Nödstopp för allt ljud
// Stoppar events via master bus
// Använd vid restart eller debug


namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    public sealed class FmodStopAllEvents : MonoBehaviour
    {
        [SerializeField] private bool immediate;

        public void StopAll()
        {
            if (AudioSystem.I != null)
                AudioSystem.I.StopAllEvents(immediate);
        }
    }
}
