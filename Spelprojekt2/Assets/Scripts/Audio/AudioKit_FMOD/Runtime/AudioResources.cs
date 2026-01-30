using UnityEngine;

// AudioKit anteckning
// Hämtar assets från Resources
// Om config saknas kör den med defaults
// Footsteps hämtas här också


namespace AudioKit.FMOD
{
    public static class AudioResources
    {
        private const string ConfigPath = "Audio/AudioConfig";
        private const string FootstepsPath = "Audio/FootstepLibrary";

        private static AudioConfigSO config;
        private static FootstepLibrarySO footsteps;

        public static AudioConfigSO Config
        {
            get
            {
                if (config != null) return config;
                config = Resources.Load<AudioConfigSO>(ConfigPath);

                if (config == null)
                {
                    config = ScriptableObject.CreateInstance<AudioConfigSO>();
                    config.name = "AudioConfig_RuntimeDefaults";
                }

                return config;
            }
        }

        public static FootstepLibrarySO Footsteps
        {
            get
            {
                if (footsteps != null) return footsteps;
                footsteps = Resources.Load<FootstepLibrarySO>(FootstepsPath);
                return footsteps;
            }
        }
    }
}
