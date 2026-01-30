using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

// AudioKit anteckning
// Laddar banker vid start
// Bra för builds och streaming
// Kan stängas av om ni inte vill


namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    public sealed class FmodBankLoader : MonoBehaviour
    {
        [SerializeField] private string[] bankNames = new string[0];
        [SerializeField] private bool loadOnStart = true;

        private readonly List<global::FMOD.Studio.Bank> loaded = new List<global::FMOD.Studio.Bank>(8);


        private void Start()
        {
            if (loadOnStart) LoadBanks();
        }

        private void OnDestroy()
        {
            UnloadBanks();
        }

        public void LoadBanks()
        {
            UnloadBanks();

            for (int i = 0; i < bankNames.Length; i++)
            {
                var name = bankNames[i];
                if (string.IsNullOrEmpty(name)) continue;

                RuntimeManager.StudioSystem.loadBankFile(
                    name,
                    global::FMOD.Studio.LOAD_BANK_FLAGS.NORMAL,
                    out var bank
                );

            }
        }

        public void UnloadBanks()
        {
            for (int i = 0; i < loaded.Count; i++)
                loaded[i].unload();

            loaded.Clear();
        }
    }
}
