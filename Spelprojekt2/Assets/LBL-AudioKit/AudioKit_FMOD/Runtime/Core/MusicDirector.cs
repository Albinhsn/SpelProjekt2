using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using AOT;

using STOP_MODE = global::FMOD.Studio.STOP_MODE;

// AudioKit anteckning
// Driver huvudmusik-eventet
// Stöder både:
// - vertikalt (layers via parametrar)
// - horisontellt (sections via state-parameter)
// Param-namn kan vara key (via AudioParamLibrary) eller direkt namn

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
            if (!inst.isValid()) return default;
            if (string.IsNullOrEmpty(name)) return default;

            inst.getDescription(out var desc);
            if (!desc.isValid()) return default;

            var res = desc.getParameterDescriptionByName(name, out var pd);
            if (res != global::FMOD.RESULT.OK) return default;

            return new CachedParam { valid = true, id = pd.id };
        }

        public static void Set(EventInstance inst, CachedParam p, float v)
        {
            if (!inst.isValid()) return;
            if (!p.valid) return;
            inst.setParameterByID(p.id, v);
        }
    }

    [DisallowMultipleComponent]
    [AddComponentMenu("AudioKit/Musik/Music Director")]
    public sealed class MusicDirector : MonoBehaviour
    {
        public static MusicDirector I { get; private set; }

        private AudioConfigSO cfg;
        private EventInstance music;
        private bool started;

        private CachedParam pState;
        private CachedParam pIntensity;
        private CachedParam pDanger;

        private readonly Dictionary<string, CachedParam> extraParams = new Dictionary<string, CachedParam>(8);
        private readonly Dictionary<string, float> pendingParams = new Dictionary<string, float>(8);

        private int barSignal;

        private bool pendState;
        private MusicState nextState;

        private bool pendIntensity;
        private float nextIntensity;

        private bool pendDanger;
        private float nextDanger;

        private GCHandle handle;
        private EVENT_CALLBACK cb;

        [Header("Main music (override valfritt)")]
        [Tooltip("Om satt används detta istället för AudioConfig.mainMusicEvent/mainMusicPath")]
        [SerializeField] private EventReference overrideMusicEvent;

        [Tooltip("Fallback path om overrideMusicEvent är tom. Ex: 'event:/Music/Main'")]
        [SerializeField] private string overrideMusicPath = "";

        [Header("Common music params (valfritt)")]
        [Tooltip("State/section-parameter (event-local). Sätt via asset, key eller direkt FMOD-namn.")]
        [SerializeField] private AudioParamRef stateParam;

        [Tooltip("Intensity/layer-parameter (event-local).")]
        [SerializeField] private AudioParamRef intensityParam;

        [Tooltip("Danger/layer-parameter (event-local).")]
        [SerializeField] private AudioParamRef dangerParam;

        private void Awake()
        {
            if (I != null && I != this)
            {
                Destroy(gameObject);
                return;
            }

            I = this;
        }

        private void Start()
        {
            cfg = AudioResources.Config;
            if (cfg == null) return;

            var useEvt = !overrideMusicEvent.IsNull ? overrideMusicEvent : cfg.mainMusicEvent;
            var usePath = !string.IsNullOrEmpty(overrideMusicPath) ? overrideMusicPath : cfg.mainMusicPath;
            if (useEvt.IsNull && string.IsNullOrEmpty(usePath)) return;

            music = useEvt.IsNull
                ? RuntimeManager.CreateInstance(usePath)
                : RuntimeManager.CreateInstance(useEvt);

            var lib = AudioResources.ParamLibrary;

            var stateName = AudioParamUtil.Resolve(stateParam.Resolve(lib));
            var intensityName = AudioParamUtil.Resolve(intensityParam.Resolve(lib));
            var dangerName = AudioParamUtil.Resolve(dangerParam.Resolve(lib));

            pState = ParamCache.Resolve(music, stateName);
            pIntensity = ParamCache.Resolve(music, intensityName);
            pDanger = ParamCache.Resolve(music, dangerName);

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

        // Snabbvägar (om ni kör klassiska 3-param upplägget)
        public void SetState(MusicState s)
        {
            if (!pState.valid) return;
            ParamCache.Set(music, pState, (float)s);
        }

        public void SetIntensity(float t)
        {
            if (!pIntensity.valid) return;
            ParamCache.Set(music, pIntensity, Mathf.Clamp01(t));
        }

        public void SetDanger(float d)
        {
            if (!pDanger.valid) return;
            ParamCache.Set(music, pDanger, Mathf.Clamp01(d));
        }

        public void SetStateQuantized(MusicState s) { pendState = true; nextState = s; }
        public void SetIntensityQuantized(float t) { pendIntensity = true; nextIntensity = Mathf.Clamp01(t); }
        public void SetDangerQuantized(float d) { pendDanger = true; nextDanger = Mathf.Clamp01(d); }

        // Generiskt (valfri parameter)
        public void SetParam(string keyOrName, float value)
        {
            // key -> FMOD-namn (från AudioParamLibrary) eller direkt namn
            var lib = AudioResources.ParamLibrary;
            var resolved = lib != null ? lib.Resolve(keyOrName) : keyOrName;
            resolved = AudioParamUtil.Resolve(resolved);

            if (string.IsNullOrEmpty(resolved)) return;
            if (!music.isValid()) return;

            var cp = GetOrResolveParam(resolved);
            ParamCache.Set(music, cp, value);
        }

        public void SetParamQuantized(string keyOrName, float value)
        {
            // key -> FMOD-namn (från AudioParamLibrary) eller direkt namn
            var lib = AudioResources.ParamLibrary;
            var resolved = lib != null ? lib.Resolve(keyOrName) : keyOrName;
            resolved = AudioParamUtil.Resolve(resolved);

            if (string.IsNullOrEmpty(resolved)) return;

            pendingParams[resolved] = value;
        }

        private CachedParam GetOrResolveParam(string name)
        {
            if (extraParams.TryGetValue(name, out var cp) && cp.valid)
                return cp;

            cp = ParamCache.Resolve(music, name);
            extraParams[name] = cp;
            return cp;
        }

        private void ApplyPendingOnBar()
        {
            if (pendState)
            {
                pendState = false;
                ParamCache.Set(music, pState, (float)nextState);
            }

            if (pendIntensity)
            {
                pendIntensity = false;
                ParamCache.Set(music, pIntensity, nextIntensity);
            }

            if (pendDanger)
            {
                pendDanger = false;
                ParamCache.Set(music, pDanger, nextDanger);
            }

            if (pendingParams.Count == 0) return;

            foreach (var kv in pendingParams)
            {
                var cp = GetOrResolveParam(kv.Key);
                ParamCache.Set(music, cp, kv.Value);
            }

            pendingParams.Clear();
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
