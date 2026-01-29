using System;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace SP2.Audio
{
    // Tanken: MusicState/Intensity/Danger sätts via event-local params.
    // Om QuantizeOnBar är på, skjuts ändringar till nästa takt-start (beat 1).
    [DefaultExecutionOrder(-80)]
    public sealed class MusicDirector : MonoBehaviour
    {
        private struct CachedParam
        {
            public bool isValid;
            public PARAMETER_ID id;
        }

        private AudioConfigSO _cfg;
        private EventInstance _music;
        private bool _started;

        private CachedParam _pState;
        private CachedParam _pIntensity;
        private CachedParam _pDanger;

        private bool _pendingState;
        private bool _pendingIntensity;
        private bool _pendingDanger;

        private MusicState _nextState;
        private float _nextIntensity;
        private float _nextDanger;

        private int _barSignal;
        private GCHandle _gcHandle;
        private FMOD.Studio.EVENT_CALLBACK _callback;

        private void Awake()
        {
            _cfg = AudioResources.Config;
            if (_cfg == null) return;
            if (_cfg.mainMusicEvent.IsNull) return;

            _music = RuntimeManager.CreateInstance(_cfg.mainMusicEvent);

            _pState = Resolve(_music, _cfg.musicStateParam);
            _pIntensity = Resolve(_music, _cfg.intensityParam);
            _pDanger = Resolve(_music, _cfg.dangerParam);

            _callback = TimelineCallback;
            _gcHandle = GCHandle.Alloc(this);
            _music.setUserData(GCHandle.ToIntPtr(_gcHandle));
            _music.setCallback(_callback, EVENT_CALLBACK_TYPE.TIMELINE_BEAT);

            StartMusic();

            SetStateImmediate(_cfg.startMusicState);
            SetIntensityImmediate(_cfg.startIntensity);
            SetDangerImmediate(0f);
        }

        private void Update()
        {
            if (_cfg == null) return;

            if (!_cfg.quantizeOnBar)
            {
                if (HasPending())
                    ApplyPending();

                return;
            }

            if (Interlocked.Exchange(ref _barSignal, 0) > 0)
            {
                if (HasPending())
                    ApplyPending();
            }
        }

        private void OnDestroy()
        {
            // Stoppar + releasear instans
            StopMusic(true);

            if (_gcHandle.IsAllocated)
                _gcHandle.Free();
        }

        private bool HasPending() => _pendingState || _pendingIntensity || _pendingDanger;

        public void StartMusic()
        {
            if (_started) return;
            if (!_music.isValid()) return;

            _music.start();
            _started = true;
        }

        public void StopMusic(bool allowFade)
        {
            if (!_music.isValid()) return;

            _music.stop(allowFade ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
            _music.release();
            _music.clearHandle();
            _started = false;
        }

        public void SetState(MusicState state)
        {
            if (_cfg == null) return;

            if (_cfg.quantizeOnBar)
            {
                _nextState = state;
                _pendingState = true;
                return;
            }

            SetStateImmediate(state);
        }

        public void SetIntensity(float intensity01)
        {
            float v = Mathf.Clamp01(intensity01);

            if (_cfg != null && _cfg.quantizeOnBar)
            {
                _nextIntensity = v;
                _pendingIntensity = true;
                return;
            }

            SetIntensityImmediate(v);
        }

        public void SetDanger(float danger01)
        {
            float v = Mathf.Clamp01(danger01);

            if (_cfg != null && _cfg.quantizeOnBar)
            {
                _nextDanger = v;
                _pendingDanger = true;
                return;
            }

            SetDangerImmediate(v);
        }

        private void ApplyPending()
        {
            if (_pendingState) SetStateImmediate(_nextState);
            if (_pendingIntensity) SetIntensityImmediate(_nextIntensity);
            if (_pendingDanger) SetDangerImmediate(_nextDanger);

            _pendingState = _pendingIntensity = _pendingDanger = false;
        }

        private void SetStateImmediate(MusicState s)
        {
            if (!_music.isValid()) return;
            SetById(_music, _pState, (float)s);
        }

        private void SetIntensityImmediate(float v)
        {
            if (!_music.isValid()) return;
            SetById(_music, _pIntensity, v);
        }

        private void SetDangerImmediate(float v)
        {
            if (!_music.isValid()) return;
            SetById(_music, _pDanger, v);
        }

        private static CachedParam Resolve(EventInstance inst, string name)
        {
            if (!inst.isValid()) return default;
            if (string.IsNullOrEmpty(name)) return default;

            inst.getDescription(out var desc);
            if (!desc.isValid()) return default;

            var res = desc.getParameterDescriptionByName(name, out var pd);
            if (res != FMOD.RESULT.OK) return default;

            return new CachedParam { isValid = true, id = pd.id };
        }

        private static void SetById(EventInstance inst, CachedParam p, float value)
        {
            if (!inst.isValid()) return;
            if (!p.isValid) return;
            inst.setParameterByID(p.id, value);
        }

        [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
        private static FMOD.RESULT TimelineCallback(EVENT_CALLBACK_TYPE type, IntPtr eventInstancePtr, IntPtr parameterPtr)
        {
            if (type != EVENT_CALLBACK_TYPE.TIMELINE_BEAT)
                return FMOD.RESULT.OK;

            var inst = new EventInstance(eventInstancePtr);
            inst.getUserData(out var userData);
            if (userData == IntPtr.Zero)
                return FMOD.RESULT.OK;

            var handle = GCHandle.FromIntPtr(userData);
            if (!(handle.Target is MusicDirector self))
                return FMOD.RESULT.OK;

            var beat = Marshal.PtrToStructure<TIMELINE_BEAT_PROPERTIES>(parameterPtr);

            // Bar start (oftast beat == 1)
            if (beat.beat == 1)
                Interlocked.Increment(ref self._barSignal);

            return FMOD.RESULT.OK;
        }
    }
}
