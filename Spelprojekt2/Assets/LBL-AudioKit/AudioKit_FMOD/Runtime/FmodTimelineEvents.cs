using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using FMODUnity;
using EventInstance = global::FMOD.Studio.EventInstance;
using EVENT_CALLBACK = global::FMOD.Studio.EVENT_CALLBACK;
using EVENT_CALLBACK_TYPE = global::FMOD.Studio.EVENT_CALLBACK_TYPE;
using STOP_MODE = global::FMOD.Studio.STOP_MODE;

// AudioKit anteckning
// FMOD timeline-callback router:
// - Lyssnar på TIMELINE_MARKER och TIMELINE_BEAT
// - Köar upp data till Unity main thread och triggar UnityEvents
// Används för att synka VFX, gameplay-ticks, musiktransitions m.m.

namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    [AddComponentMenu("AudioKit/Debug/FMOD Timeline Events")]
    public sealed class FmodTimelineEvents : MonoBehaviour
    {
        [Serializable] public class MarkerEvent : UnityEvent<string, int> { }
        [Serializable] public class BeatEvent : UnityEvent<int, int, int, float> { }

        [Header("Event")]
        [SerializeField] private EventReference evt;
        [SerializeField] private bool playOnEnable = false;
        [SerializeField] private bool is2D = true;

        [Header("Callbacks")]
        public MarkerEvent onMarker;
        public BeatEvent onBeat;

        private EventInstance inst;

        private GCHandle gcHandle;
        private TimelineInfo info;

        private readonly Queue<(string name, int pos)> markerQueue = new Queue<(string, int)>(32);
        private readonly Queue<(int bar, int beat, int pos, float tempo)> beatQueue = new Queue<(int, int, int, float)>(64);
        private readonly object qLock = new object();

        private static readonly EVENT_CALLBACK Callback = TimelineCallback;

        private void OnEnable()
        {
            if (playOnEnable) Play();
        }

        private void OnDisable()
        {
            Stop();
        }

        private void OnDestroy()
        {
            Stop();
        }

        public void Play()
        {
            if (evt.IsNull) return;

            if (!inst.isValid())
            {
                inst = RuntimeManager.CreateInstance(evt);
                if (!is2D)
                    RuntimeManager.AttachInstanceToGameObject(inst, gameObject, GetComponent<Rigidbody>());

                info = new TimelineInfo(this);
                gcHandle = GCHandle.Alloc(info, GCHandleType.Pinned);
                inst.setUserData(GCHandle.ToIntPtr(gcHandle));

                inst.setCallback(Callback, EVENT_CALLBACK_TYPE.TIMELINE_MARKER | EVENT_CALLBACK_TYPE.TIMELINE_BEAT);
            }

            inst.start();
        }

        public void Stop()
        {
            if (inst.isValid())
            {
                inst.stop(STOP_MODE.ALLOWFADEOUT);
                inst.release();
                inst.clearHandle();
                inst = default;
            }

            if (gcHandle.IsAllocated)
                gcHandle.Free();

            info = null;

            lock (qLock)
            {
                markerQueue.Clear();
                beatQueue.Clear();
            }
        }

        private void Update()
        {
            // Drain queued events on main thread
            if (onMarker != null)
            {
                lock (qLock)
                {
                    while (markerQueue.Count > 0)
                    {
                        var (n, p) = markerQueue.Dequeue();
                        onMarker.Invoke(n, p);
                    }
                }
            }
            else
            {
                lock (qLock) markerQueue.Clear();
            }

            if (onBeat != null)
            {
                lock (qLock)
                {
                    while (beatQueue.Count > 0)
                    {
                        var (bar, beat, pos, tempo) = beatQueue.Dequeue();
                        onBeat.Invoke(bar, beat, pos, tempo);
                    }
                }
            }
            else
            {
                lock (qLock) beatQueue.Clear();
            }
        }

        private void EnqueueMarker(string name, int pos)
        {
            lock (qLock)
                markerQueue.Enqueue((name, pos));
        }

        private void EnqueueBeat(int bar, int beat, int pos, float tempo)
        {
            lock (qLock)
                beatQueue.Enqueue((bar, beat, pos, tempo));
        }

        [AOT.MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
        private static global::FMOD.RESULT TimelineCallback(EVENT_CALLBACK_TYPE type, IntPtr eventInstance, IntPtr parameters)
        {
            var inst = new EventInstance(eventInstance);

            inst.getUserData(out var userData);
            if (userData == IntPtr.Zero) return global::FMOD.RESULT.OK;

            GCHandle handle = GCHandle.FromIntPtr(userData);
            if (!(handle.Target is TimelineInfo tl)) return global::FMOD.RESULT.OK;

            if (type == EVENT_CALLBACK_TYPE.TIMELINE_MARKER)
            {
                var marker = (TIMELINE_MARKER_PROPERTIES)Marshal.PtrToStructure(parameters, typeof(TIMELINE_MARKER_PROPERTIES));
                var name = Marshal.PtrToStringAnsi(marker.name) ?? string.Empty;
                tl.owner?.EnqueueMarker(name, marker.position);
            }
            else if (type == EVENT_CALLBACK_TYPE.TIMELINE_BEAT)
            {
                var beat = (TIMELINE_BEAT_PROPERTIES)Marshal.PtrToStructure(parameters, typeof(TIMELINE_BEAT_PROPERTIES));
                tl.owner?.EnqueueBeat(beat.bar, beat.beat, beat.position, beat.tempo);
            }

            return global::FMOD.RESULT.OK;
        }

        private sealed class TimelineInfo
        {
            public readonly FmodTimelineEvents owner;
            public TimelineInfo(FmodTimelineEvents owner) { this.owner = owner; }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TIMELINE_BEAT_PROPERTIES
        {
            public int bar;
            public int beat;
            public int position;
            public float tempo;
            public int timesignatureupper;
            public int timesignaturelower;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TIMELINE_MARKER_PROPERTIES
        {
            public int position;
            public IntPtr name;
        }
    }
}
