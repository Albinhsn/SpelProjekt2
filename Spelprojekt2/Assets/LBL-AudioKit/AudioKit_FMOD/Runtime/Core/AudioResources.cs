using UnityEngine;

// AudioKit anteckning
// Hämtar assets från Resources/Audio
// Om något saknas kör vi med safe defaults

namespace AudioKit.FMOD
{
    public static class AudioResources
    {
        private const string ConfigPath = "Audio/AudioConfig";
        private const string FootstepsPath = "Audio/FootstepLibrary";
        private const string ParamLibraryPath = "Audio/AudioParamLibrary";
        private const string EventRegistryPath = "Audio/EventRegistry";

        private static AudioConfigSO config;
        private static FootstepLibrarySO footsteps;
        private static AudioParamLibrarySO paramLibrary;
        private static AudioEventRegistrySO eventRegistry;

        public static AudioConfigSO Config
        {
            get
            {
                if (config == null) config = Resources.Load<AudioConfigSO>(ConfigPath);
                return config;
            }
        }

        public static FootstepLibrarySO Footsteps
        {
            get
            {
                if (footsteps == null) footsteps = Resources.Load<FootstepLibrarySO>(FootstepsPath);
                return footsteps;
            }
        }

        public static AudioParamLibrarySO ParamLibrary
        {
            get
            {
                if (paramLibrary == null) paramLibrary = Resources.Load<AudioParamLibrarySO>(ParamLibraryPath);
                return paramLibrary;
            }
        }

        public static AudioEventRegistrySO EventRegistry
        {
            get
            {
                if (eventRegistry == null) eventRegistry = Resources.Load<AudioEventRegistrySO>(EventRegistryPath);
                return eventRegistry;
            }
        }
    }
}
