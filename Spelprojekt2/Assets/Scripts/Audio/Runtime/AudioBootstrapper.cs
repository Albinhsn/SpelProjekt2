using UnityEngine;

namespace SP2.Audio
{
    public static class AudioBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            if (AudioSystem.Instance != null)
                return;

            var go = new GameObject("AudioSystem");
            go.AddComponent<AudioSystem>();
        }
    }
}
