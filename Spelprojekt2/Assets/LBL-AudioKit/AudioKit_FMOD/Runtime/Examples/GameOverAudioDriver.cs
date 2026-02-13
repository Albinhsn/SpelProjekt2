using UnityEngine;

namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    public sealed class GameOverAudioDriver : MonoBehaviour
    {
        [Header("Parameter")]
        [Tooltip("Sätt en global parameter för Game Over. Använd asset/key/namn (AudioParamRef).")]
        [SerializeField] private AudioParamRef gameOverParam;

        [SerializeField] private float onValue = 1f;
        [SerializeField] private float offValue = 0f;

        [SerializeField] private bool stopAllEvents;

        public void TriggerGameOver()
        {
            if (AudioSystem.I != null)
            {
                var name = gameOverParam.Resolve(AudioResources.ParamLibrary);
                if (!string.IsNullOrEmpty(name))
                    AudioSystem.I.SetGlobalParam(name, onValue);
            }

            if (stopAllEvents && AudioSystem.I != null)
                AudioSystem.I.StopAllEvents(false);
        }

        public void ClearGameOver()
        {
            if (AudioSystem.I != null)
            {
                var name = gameOverParam.Resolve(AudioResources.ParamLibrary);
                if (!string.IsNullOrEmpty(name))
                    AudioSystem.I.SetGlobalParam(name, offValue);
            }
        }
    }
}
