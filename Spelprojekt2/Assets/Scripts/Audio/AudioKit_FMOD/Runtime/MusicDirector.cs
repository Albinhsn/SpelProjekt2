using System;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using AOT;

using STOP_MODE = global::FMOD.Studio.STOP_MODE;
// AudioKit anteckning
// Driver huvudmusiken
// State och layers via parametrar
// Kan byta på takt om du vill

namespace AudioKit.FMOD
{
    public enum MusicState
    {
        Menu = 0,
        Explore = 1,
        Encounter = 2
    }

    internal struct CachedParam
    {
        public bool valid;
        public PARAMETER_ID id;
    }

    internal static class ParamCache
    {
        public static CachedParam Resolve(EventInstance inst, string name)
        {
            if (!inst.isValid()) return default(CachedParam);
            if (string.IsNullOrEmpty(name)) return default(CachedParam);

            inst.getDescription(out var desc);
            if (!desc.isValid()) return default(CachedParam);

            var res = desc.getParameterDescriptionByName(name, out var pd);
            if (res != global::FMOD.RESULT.OK) return default(CachedParam);

            var cp = new CachedParam();
            cp.valid = true;
            cp.id = pd.id;
            return cp;
        }

        public static void Set(EventInstance inst, CachedParam p, float v)
        {
            if (!inst.isValid()) return;
            if (!p.valid) return;
            inst.setParameterByID(p.id, v);
        }
    }

    [DisallowMultipleComponent]
    public sealed class MusicDirector : MonoBehaviour
    {
        public static MusicDirector I { get; private set; }

        private AudioConfigSO cfg;

        private EventInstance music;
        private bool started;

        private CachedParam pState;
        private CachedParam pIntensity;
        private CachedParam pDanger;

        private int barSignal;

        private bool pendState;
        private MusicState nextState;

        private bool pendIntensity;
        private float nextIntensity;

        private bool pendDanger;
        private float nextDanger;

        private GCHandle handle;
        private EVENT_CALLBACK cb;

        private void Awake()
        {
            if (I != null) { Destroy(gameObject); return; }
            I = this;
        }

        private void Start()
        {
            cfg = AudioResources.Config;
            if (cfg == null) return;
            if (cfg.mainMusicEvent.IsNull && string.IsNullOrEmpty(cfg.mainMusicPath)) return;

            music = cfg.mainMusicEvent.IsNull
                ? RuntimeManager.CreateInstance(cfg.mainMusicPath)
                : RuntimeManager.CreateInstance(cfg.mainMusicEvent);

            pState = ParamCache.Resolve(music, cfg.musicStateParam);
            pIntensity = ParamCache.Resolve(music, cfg.intensityParam);
            pDanger = ParamCache.Resolve(music, cfg.dangerParam);

            cb = TimelineCallback;
            handle = GCHandle.Alloc(this);
            music.setUserData(GCHandle.ToIntPtr(handle));
            music.setCallback(cb, EVENT_CALLBACK_TYPE.TIMELINE_BEAT);

            StartMusic();
        }

        private void Update()
        {
            if (Interlocked.Exchange(ref barSignal, 0) > 0)
                ApplyPendingOnBar();
        }

        private void OnDestroy()
        {
            if (I != this) return;

            StopMusic(true);

            if (handle.IsAllocated)
                handle.Free();

            I = null;
        }

        public void StartMusic()
        {
            if (!music.isValid()) return;
            if (started) return;
            music.start();
            started = true;
        }

        public void StopMusic(bool fade)
        {
            if (!music.isValid()) return;

            music.stop(fade ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
            music.release();
            music.clearHandle();
            started = false;
        }

        public void SetState(MusicState s) => ParamCache.Set(music, pState, (float)s);
        public void SetIntensity(float t) => ParamCache.Set(music, pIntensity, Mathf.Clamp01(t));
        public void SetDanger(float d) => ParamCache.Set(music, pDanger, Mathf.Clamp01(d));

        public void SetStateQuantized(MusicState s) { pendState = true; nextState = s; }
        public void SetIntensityQuantized(float t) { pendIntensity = true; nextIntensity = Mathf.Clamp01(t); }
        public void SetDangerQuantized(float d) { pendDanger = true; nextDanger = Mathf.Clamp01(d); }

        private void ApplyPendingOnBar()
        {
            if (pendState) { pendState = false; ParamCache.Set(music, pState, (float)nextState); }
            if (pendIntensity) { pendIntensity = false; ParamCache.Set(music, pIntensity, nextIntensity); }
            if (pendDanger) { pendDanger = false; ParamCache.Set(music, pDanger, nextDanger); }
        }

        [MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
        private static global::FMOD.RESULT TimelineCallback(EVENT_CALLBACK_TYPE type, IntPtr instPtr, IntPtr paramPtr)
        {
            if (type != EVENT_CALLBACK_TYPE.TIMELINE_BEAT)
                return global::FMOD.RESULT.OK;

            var inst = new EventInstance(instPtr);
            inst.getUserData(out var ud);
            if (ud == IntPtr.Zero) return global::FMOD.RESULT.OK;

            var h = GCHandle.FromIntPtr(ud);
            var self = h.Target as MusicDirector;
            if (self == null) return global::FMOD.RESULT.OK;

            var beat = Marshal.PtrToStructure<TIMELINE_BEAT_PROPERTIES>(paramPtr);
            if (beat.beat == 1)
                Interlocked.Increment(ref self.barSignal);

            return global::FMOD.RESULT.OK;
        }
    }
}
