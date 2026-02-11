using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using EventInstance = global::FMOD.Studio.EventInstance;
using STOP_MODE = global::FMOD.Studio.STOP_MODE;

// AudioKit anteckning
// Enkel hub för events via ID (AudioEventRegistry)
// Bra för att slippa hårdkoda paths överallt

namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    [AddComponentMenu("AudioKit/Core/Audio Event Hub")]
    public sealed class AudioEventHub : MonoBehaviour
    {
        public static AudioEventHub I { get; private set; }

        [SerializeField] private bool dontDestroyOnLoad = true;

        private AudioEventRegistrySO registry;
        private readonly Dictionary<string, EventInstance> loops = new Dictionary<string, EventInstance>(32);

        private readonly Dictionary<string, bool> globalParamCache = new Dictionary<string, bool>(64);

        private void Awake()
        {
            if (I != null) { Destroy(gameObject); return; }
            I = this;

            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);

            registry = AudioResources.EventRegistry;
            
        }

        private void OnDestroy()
        {
            if (I != this) return;

            foreach (var kv in loops)
            {
                var inst = kv.Value;
                if (!inst.isValid()) continue;
                inst.stop(STOP_MODE.ALLOWFADEOUT);
                inst.release();
            }

            loops.Clear();
            globalParamCache.Clear();
            I = null;
        }

        public bool HasRegistry => registry != null;

        public bool TryGet(string eventId, out EventInstance inst)
        {
            return loops.TryGetValue(eventId, out inst) && inst.isValid();
        }

        public void PlayOneShot(string eventId, Vector3 pos)
        {
            if (registry == null) return;
            if (!registry.TryGetEvent(eventId, out var ev)) return;

            RuntimeManager.PlayOneShot(ev, pos);
        }

        public void StartLoop(string eventId)
        {
            if (registry == null) return;
            if (string.IsNullOrEmpty(eventId)) return;

            if (loops.TryGetValue(eventId, out var existing) && existing.isValid())
                return;

            if (!registry.TryGetEvent(eventId, out var ev)) return;

            var inst = RuntimeManager.CreateInstance(ev);
            inst.start();
            loops[eventId] = inst;
        }

        // Legacy alias
        public void PlayIfNotPlaying(string eventId) => StartLoop(eventId);

        // Legacy alias
        public void StopIfPlaying(string eventId, bool fadeOut = true) => StopLoop(eventId, fadeOut);

        public void StopLoop(string eventId, bool fadeOut = true)
        {
            if (string.IsNullOrEmpty(eventId)) return;

            if (!loops.TryGetValue(eventId, out var inst)) return;
            loops.Remove(eventId);

            if (!inst.isValid()) return;
            inst.stop(fadeOut ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
            inst.release();
        }

        public void SetGlobalParam(string keyOrName, float value)
        {
            AudioSystem.I?.SetGlobalParam(keyOrName, value);
        }

        public void SetEventParam(string eventId, string keyOrName, float value)
        {
            if (string.IsNullOrEmpty(eventId)) return;

            var pName = AudioParamUtil.Resolve(keyOrName);
            if (string.IsNullOrEmpty(pName)) return;

            if (IsGlobalParamCached(pName))
            {
                SetGlobalParam(pName, value);
                return;
            }

            if (!loops.TryGetValue(eventId, out var inst) || !inst.isValid()) return;
            inst.setParameterByName(pName, value);
        }

        private bool IsGlobalParamCached(string paramName)
        {
            if (string.IsNullOrEmpty(paramName)) return false;

            if (globalParamCache.TryGetValue(paramName, out var cached))
                return cached;

            var isGlobal = false;
            var res = RuntimeManager.StudioSystem.getParameterDescriptionByName(paramName, out _);
			if (res == global::FMOD.RESULT.OK)
                isGlobal = true;

            globalParamCache[paramName] = isGlobal;
            return isGlobal;
        }
        // Convenience aliases (för enklare migration från äldre scripts)
        public void Play(string eventId) => StartLoop(eventId);

        public void Stop(string eventId, bool fadeOut = true) => StopLoop(eventId, fadeOut);

        // Fade = 0 => IMMEDIATE, annars ALLOWFADEOUT (FMOD hanterar själva fade-kurvan i eventet)
        public void Stop(string eventId, float fade)
        {
            var allow = fade > 0.001f;
            StopLoop(eventId, allow);
        }

        // One-shot direkt via EventReference (t.ex. cues)
        public void PlayOneShot(EventReference evt, Vector3 pos, bool is2D = false)
        {
            if (evt.IsNull) return;

            if (is2D) RuntimeManager.PlayOneShot(evt);
            else RuntimeManager.PlayOneShot(evt, pos);
        }


        public int ActiveLoopCount
        {
            get
            {
                var c = 0;
                foreach (var kv in loops)
                    if (kv.Value.isValid()) c++;
                return c;
            }
        }

        public void GetActiveLoopIds(List<string> outIds)
        {
            if (outIds == null) return;
            outIds.Clear();

            foreach (var kv in loops)
            {
                if (!kv.Value.isValid()) continue;
                outIds.Add(kv.Key);
            }
        }
    }
}
