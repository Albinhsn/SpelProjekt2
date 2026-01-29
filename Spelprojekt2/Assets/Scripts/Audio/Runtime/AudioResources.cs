using UnityEngine;

namespace SP2.Audio
{
    public static class AudioResources
    {
        private const string CONFIG_PATH = "Audio/AudioConfig";
        private const string FOOTSTEPS_PATH = "Audio/FootstepLibrary";

        private static AudioConfigSO _config;
        private static FootstepLibrarySO _footsteps;

        public static AudioConfigSO Config
        {
            get
            {
                if (_config != null)
                    return _config;

                _config = Resources.Load<AudioConfigSO>(CONFIG_PATH);
                if (_config == null)
                {
                    Debug.LogError($"[AudioResources] Hittar inte AudioConfig. Skapa: Resources/{CONFIG_PATH}.asset");
                }

                return _config;
            }
        }

        public static FootstepLibrarySO Footsteps
        {
            get
            {
                if (_footsteps != null)
                    return _footsteps;

                _footsteps = Resources.Load<FootstepLibrarySO>(FOOTSTEPS_PATH);
                if (_footsteps == null)
                {
                    Debug.LogWarning($"[AudioResources] Hittar ingen FootstepLibrary (valfritt): Resources/{FOOTSTEPS_PATH}.asset");
                }

                return _footsteps;
            }
        }
    }
}
