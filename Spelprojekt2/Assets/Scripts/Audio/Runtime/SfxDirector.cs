using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace SP2.Audio
{
    [DefaultExecutionOrder(-70)]
    public sealed class SfxDirector : MonoBehaviour
    {
        private readonly Dictionary<int, float> _lastPlay = new();

        public void Play(AudioCueSO cue, Vector3 worldPos)
        {
            if (cue == null) return;
            if (cue.eventReference.IsNull) return;
            if (!CooldownOk(cue)) return;

            if (cue.is2D) FmodSafe.PlayOneShot2DSafe(cue.eventReference);
            else FmodSafe.PlayOneShotSafe(cue.eventReference, worldPos);
        }

        public void Play2D(AudioCueSO cue)
        {
            Play(cue, Vector3.zero);
        }

        public EventInstance CreateAttachedInstance(AudioCueSO cue, Transform target, Rigidbody rb = null)
        {
            if (cue == null) return default;
            if (cue.eventReference.IsNull) return default;
            if (target == null) return default;

            if (!FmodSafe.EventExists(cue.eventReference))
                return default;

            var inst = RuntimeManager.CreateInstance(cue.eventReference);
            RuntimeManager.AttachInstanceToGameObject(inst, target, rb);
            return inst;
        }

        private bool CooldownOk(AudioCueSO cue)
        {
            if (cue.cooldownSeconds <= 0f)
                return true;

            int id = cue.GetInstanceID();
            float now = Time.unscaledTime;

            if (_lastPlay.TryGetValue(id, out float t))
            {
                if (now - t < cue.cooldownSeconds)
                    return false;
            }

            _lastPlay[id] = now;
            return true;
        }
    }
}
