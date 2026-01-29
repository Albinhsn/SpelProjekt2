using UnityEngine;
using FMOD.Studio;
using FMODUnity;

namespace SP2.Audio
{
    // Stops everything on the master bus (useful for GameOver / hard resets).

    public sealed class FmodStopAllEvents : MonoBehaviour
    {
        [SerializeField] private bool allowFadeOut = true;

        public void StopAll()
        {
            var cfg = AudioResources.Config;
            string busPath = (cfg != null && !string.IsNullOrEmpty(cfg.busMaster)) ? cfg.busMaster : "bus:/";

            Bus bus = RuntimeManager.GetBus(busPath);
            if (!bus.isValid()) return;

            bus.stopAllEvents(allowFadeOut ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
        }

        public void StopAllImmediate()
        {
            allowFadeOut = false;
            StopAll();
        }
    }
}
