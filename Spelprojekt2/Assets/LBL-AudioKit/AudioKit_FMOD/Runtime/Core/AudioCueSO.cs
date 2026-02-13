using System;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

// AudioKit anteckning
// En cue för ett ljud
// 2D eller 3D, cooldown och preload
// Används av triggers och emitters


namespace AudioKit.FMOD
{
    [CreateAssetMenu(menuName = "AudioKit/Ljud/AudioCue", fileName = "AC_")]
    public sealed class AudioCueSO : ScriptableObject
    {
        [Header("Event")]
        public EventReference evt;

        [Header("Speltyp")]
        public bool is2D;

        [Header("Skydd mot spam")]
        [Min(0f)] public float cooldown;

        [Header("Preload")]
        public bool preloadSampleData;

        [NonSerialized] internal float nextPlayableTime;
        [NonSerialized] internal bool sampleLoaded;
        [NonSerialized] internal EventDescription cachedDesc;

        internal void EnsureSampleDataLoaded()
        {
            if (!preloadSampleData) return;
            if (sampleLoaded) return;
            if (evt.IsNull) return;

            cachedDesc = RuntimeManager.GetEventDescription(evt);
            if (cachedDesc.isValid())
            {
                cachedDesc.loadSampleData();
                sampleLoaded = true;
            }
        }

        internal void EnsureSampleDataUnloaded()
        {
            if (!sampleLoaded) return;

            if (cachedDesc.isValid())
            {
                cachedDesc.unloadSampleData();
            }

            sampleLoaded = false;
        }
    }
}
