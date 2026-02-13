using System;
using UnityEngine;

// AudioKit anteckning
// Flexibel referens till en parameter
// Val sker via asset, key (AudioParamLibrary) eller direkt namn

namespace AudioKit.FMOD
{
    [Serializable]
    public struct AudioParamRef
    {
        [Tooltip("Valfritt: om satt används fmodName från asseten")]
        public AudioParameterSO parameter;

        [Tooltip("Valfritt: nyckel som slås upp i AudioParamLibrary")]
        public string key;

        [Tooltip("Valfritt: direkt FMOD-namn")]
        public string name;

        public string Resolve(AudioParamLibrarySO lib)
        {
            if (parameter != null && parameter.IsValid)
                return parameter.fmodName;

            if (!string.IsNullOrEmpty(key))
                return lib != null ? lib.Resolve(key) : key;

            if (!string.IsNullOrEmpty(name))
                return name;

            return null;
        }

        public bool HasSomething
        {
            get
            {
                if (parameter != null && parameter.IsValid) return true;
                if (!string.IsNullOrEmpty(key)) return true;
                if (!string.IsNullOrEmpty(name)) return true;
                return false;
            }
        }
    }
}
