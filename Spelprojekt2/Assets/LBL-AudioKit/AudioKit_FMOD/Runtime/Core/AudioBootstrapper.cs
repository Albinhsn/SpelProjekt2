using UnityEngine;

// Startar ljudsystemet automatiskt
// Kör efter första scenen laddats så vi kan undvika dubbletter

namespace AudioKit.FMOD
{
    public static class AudioBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            // Om någon redan lagt AudioSystem i scenen: skapa inte ett till.
            if (AudioUnityFind.FindAny<AudioSystem>(true) != null)
                return;

            var go = new GameObject("_AudioSystem");
            Object.DontDestroyOnLoad(go);

            go.AddComponent<AudioSystem>();
            go.AddComponent<AudioEventHub>();
            go.AddComponent<MusicDirector>();
            go.AddComponent<SfxDirector>();
            go.AddComponent<AudioParameterDriver>();
        }
    }
}
