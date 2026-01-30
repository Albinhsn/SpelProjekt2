using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

using STOP_MODE = global::FMOD.Studio.STOP_MODE;
// AudioKit anteckning
// Central hub för att spela events via ID
// Bra när många scripts ska trigga samma ljud
// Pekar på registry

namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    public sealed class AudioEventHub : MonoBehaviour
    {
        public static AudioEventHub I { get; private set; }

        private const string RegistryPath = "Audio/EventRegistry";

        private AudioEventRegistrySO registry;

        private readonly Dictionary<string, EventInstance> instances = new Dictionary<string, EventInstance>(16);
        private readonly Dictionary<string, bool> globalParamCache = new Dictionary<string, bool>(16);

        private void Awake()
        {
            if (I != null) { Destroy(gameObject); return; }
            I = this;

            registry = Resources.Load<AudioEventRegistrySO>(RegistryPath);
            if (registry != null)
                CreateConfiguredInstances();
        }

        private void OnDestroy()
        {
            if (I != this) return;

            foreach (var kv in instances)
            {
                var inst = kv.Value;
                if (inst.isValid())
                {
                    inst.stop(STOP_MODE.IMMEDIATE);
                    inst.release();
                }
            }

            instances.Clear();
            I = null;
        }

        private void CreateConfiguredInstances()
        {
            instances.Clear();

            var list = registry.events;
            for (int i = 0; i < list.Length; i++)
            {
                var e = list[i];
                if (e == null) continue;
                if (string.IsNullOrEmpty(e.id)) continue;
                if (e.evt.IsNull) continue;

                if (!e.createInstanceOnStart) continue;

                var inst = RuntimeManager.CreateInstance(e.evt);
                instances[e.id] = inst;

                if (e.preloadSampleData)
                {
                    var desc = RuntimeManager.GetEventDescription(e.evt);
                    if (desc.isValid()) desc.loadSampleData();
                }

                if (e.startOnBoot)
                    inst.start();
            }
        }

        public bool TryGet(string id, out EventInstance inst)
        {
            inst = default(EventInstance);
            if (string.IsNullOrEmpty(id)) return false;
            if (!instances.TryGetValue(id, out inst)) return false;
            return inst.isValid();
        }

        public void Play(string id)
        {
            if (!TryGet(id, out var inst)) return;
            inst.start();
        }

        public void PlayIfNotPlaying(string id)
        {
            if (!TryGet(id, out var inst)) return;

            inst.getPlaybackState(out var st);
            if (st == PLAYBACK_STATE.STARTING || st == PLAYBACK_STATE.PLAYING || st == PLAYBACK_STATE.SUSTAINING)
                return;

            inst.start();
        }

        public void Stop(string id, bool fade)
        {
            if (!TryGet(id, out var inst)) return;
            inst.stop(fade ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
        }

        public void StopAll(bool fade)
        {
            var mode = fade ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE;

            foreach (var kv in instances)
            {
                var inst = kv.Value;
                if (inst.isValid()) inst.stop(mode);
            }
        }

        public void SetEventParam(string id, string paramName, float value)
        {
            if (string.IsNullOrEmpty(paramName)) return;
            if (IsGlobalParam(paramName)) return;

            if (!TryGet(id, out var inst)) return;
            inst.setParameterByName(paramName, value);
        }

        public void SetGlobalParam(string paramName, float value)
        {
            if (AudioSystem.I != null)
            {
                AudioSystem.I.SetGlobalParam(paramName, value);
                return;
            }

            if (string.IsNullOrEmpty(paramName)) return;
            if (!IsGlobalParam(paramName)) return;

            RuntimeManager.StudioSystem.setParameterByName(paramName, value);
        }

        private bool IsGlobalParam(string paramName)
        {
            if (string.IsNullOrEmpty(paramName)) return false;
            if (globalParamCache.TryGetValue(paramName, out var cached)) return cached;

            var res = RuntimeManager.StudioSystem.getParameterDescriptionByName(paramName, out _);
            var ok = res == global::FMOD.RESULT.OK;

            globalParamCache[paramName] = ok;
            return ok;
        }

        public void PlayOneShot(EventReference evt, Vector3 pos, bool is2D)
        {
            if (evt.IsNull) return;
            if (is2D) RuntimeManager.PlayOneShot(evt);
            else RuntimeManager.PlayOneShot(evt, pos);
        }
    }
}
