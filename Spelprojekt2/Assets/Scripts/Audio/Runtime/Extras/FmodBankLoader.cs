using UnityEngine;
using FMODUnity;

namespace SP2.Audio
{
    // Optional: explicit bank loading (if you don't want automatic bank handling).

    public sealed class FmodBankLoader : MonoBehaviour
    {
        [System.Serializable]
        public struct Bank
        {
            public string name;
            public bool loadSampleData;
        }

        [Header("== Banks ==")]
        [SerializeField] private Bank[] banks;

        [Header("== Timing ==")]
        [SerializeField] private bool loadOnAwake = true;
        [SerializeField] private bool unloadOnDestroy = false;

        private void Awake()
        {
            if (loadOnAwake)
                LoadAll();
        }

        private void OnDestroy()
        {
            if (unloadOnDestroy)
                UnloadAll();
        }

        public void LoadAll()
        {
            if (banks == null) return;

            for (int i = 0; i < banks.Length; i++)
            {
                var b = banks[i];
                if (string.IsNullOrEmpty(b.name)) continue;
                RuntimeManager.LoadBank(b.name, b.loadSampleData);
            }
        }

        public void UnloadAll()
        {
            if (banks == null) return;

            for (int i = 0; i < banks.Length; i++)
            {
                var b = banks[i];
                if (string.IsNullOrEmpty(b.name)) continue;
                RuntimeManager.UnloadBank(b.name);
            }
        }
    }
}
