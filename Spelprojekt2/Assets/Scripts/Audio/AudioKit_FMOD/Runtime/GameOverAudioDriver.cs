using UnityEngine;

namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    public sealed class GameOverAudioDriver : MonoBehaviour
    {
        [SerializeField] private bool stopAllEvents;

        public void TriggerGameOver()
        {
            var cfg = AudioResources.Config;
            if (cfg != null && AudioSystem.I != null)
                AudioSystem.I.SetGlobalParam(cfg.gameOverParam, 1f);

            if (stopAllEvents && AudioSystem.I != null)
                AudioSystem.I.StopAllEvents(false);
        }

        public void ClearGameOver()
        {
            var cfg = AudioResources.Config;
            if (cfg != null && AudioSystem.I != null)
                AudioSystem.I.SetGlobalParam(cfg.gameOverParam, 0f);
        }
    }
}
