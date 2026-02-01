namespace AudioKit.FMOD
{
    // AudioKit anteckning
    // Små helpers för att slå upp param-nycklar
    
    internal static class AudioParamUtil
    {
        public static string Resolve(string keyOrName)
        {
            if (string.IsNullOrEmpty(keyOrName)) return null;

            var lib = AudioResources.ParamLibrary;
            return lib != null ? lib.Resolve(keyOrName) : keyOrName;
        }

        public static string Resolve(AudioParamRef r)
        {
            return r.Resolve(AudioResources.ParamLibrary);
        }
    }
}
