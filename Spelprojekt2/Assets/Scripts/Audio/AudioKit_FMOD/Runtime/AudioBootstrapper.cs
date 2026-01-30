using UnityEngine;

// AudioKit anteckning
// Startar ljudsystemet automatiskt
// Slipper lägga managers i scenen
// DontDestroy så det lever mellan scener


namespace AudioKit.FMOD
{
    public static class AudioBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            var go = new GameObject("AudioSystem");
            Object.DontDestroyOnLoad(go);

            go.AddComponent<AudioSystem>();
            go.AddComponent<AudioEventHub>();
            go.AddComponent<MusicDirector>();
            go.AddComponent<SfxDirector>();
            go.AddComponent<AudioParameterDriver>();
        }
    }
}
