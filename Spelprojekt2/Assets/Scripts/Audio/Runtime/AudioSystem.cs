using UnityEngine;
using FMODUnity;

namespace SP2.Audio
{
    // Skapas via AudioBootstrapper. Ingen scene-setup.
    [DefaultExecutionOrder(-100)]
    public sealed class AudioSystem : MonoBehaviour
    {
        public static AudioSystem Instance { get; private set; }

        public AudioConfigSO Config { get; private set; }
        public AudioMixer Mixer { get; private set; }
        public MusicDirector Music { get; private set; }
        public SfxDirector Sfx { get; private set; }
        public AudioParameterDriver Params { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Config = AudioResources.Config;
            if (Config == null)
            {
                Debug.LogError("[AudioSystem] AudioConfig saknas. Skapa Resources/Audio/AudioConfig.asset");
                return;
            }

            Mixer = GetOrAdd<AudioMixer>();
            Music = GetOrAdd<MusicDirector>();
            Sfx = GetOrAdd<SfxDirector>();
            Params = GetOrAdd<AudioParameterDriver>();

            if (Config.ensureStudioListener)
                EnsureStudioListener();
        }

        private static T GetOrAdd<T>() where T : Component
        {
            if (Instance == null)
                return null;

            if (Instance.TryGetComponent<T>(out var c))
                return c;

            return Instance.gameObject.AddComponent<T>();
        }

        private static void EnsureStudioListener()
        {
#if UNITY_2023_1_OR_NEWER
            if (FindFirstObjectByType<StudioListener>() != null)
                return;
#else
            if (FindObjectOfType<StudioListener>() != null)
                return;
#endif

            Camera cam = Camera.main;
            if (cam == null)
            {
#if UNITY_2023_1_OR_NEWER
                cam = FindFirstObjectByType<Camera>();
#else
                cam = FindObjectOfType<Camera>();
#endif
            }

            if (cam != null)
            {
                cam.gameObject.AddComponent<StudioListener>();
                return;
            }

            // Fallback: skapa en listener i världen (typ om man startar utan kamera)
            var go = new GameObject("FMOD StudioListener");
            go.AddComponent<StudioListener>();
            DontDestroyOnLoad(go);
        }
    }
}
