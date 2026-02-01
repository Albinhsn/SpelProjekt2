using System.Collections.Generic;
using UnityEngine;
using FMODUnity;


// Laddar banker vid start
// Bra för builds och streaming
// OBS: använd bank-NAMN som FMOD Unity känner igen (t.ex. "Master", "Music", "SFX")

namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    public sealed class FmodBankLoader : MonoBehaviour
    {
        [SerializeField] private string[] bankNames = System.Array.Empty<string>();
        [SerializeField] private bool loadOnStart = true;
        [SerializeField] private bool unloadOnDestroy = true;
        [SerializeField] private bool loadSampleData;

        private readonly HashSet<string> loaded = new HashSet<string>();

        private void Start()
        {
            if (loadOnStart) LoadBanks();
        }

        private void OnDestroy()
        {
            if (unloadOnDestroy) UnloadBanks();
        }

        public void LoadBanks()
        {
            UnloadBanks();

            for (int i = 0; i < bankNames.Length; i++)
            {
                var name = bankNames[i];
                if (string.IsNullOrEmpty(name)) continue;

                RuntimeManager.LoadBank(name, loadSampleData);
                loaded.Add(name);
            }
        }

        public void UnloadBanks()
        {
            foreach (var name in loaded)
                RuntimeManager.UnloadBank(name);

            loaded.Clear();
        }
    }
}
