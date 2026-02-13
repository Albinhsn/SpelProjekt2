using UnityEngine;
using FMODUnity;
using FMOD.Studio;

using STOP_MODE = global::FMOD.Studio.STOP_MODE;
// AudioKit anteckning
// Spelar SFX cues
// Håller ordning på instances
// Attach till objekt när det behövs

namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    public sealed class SfxDirector : MonoBehaviour
    {
        public static SfxDirector I { get; private set; }

        private void Awake()
        {
            if (I != null) { Destroy(gameObject); return; }
            I = this;
        }

        private void OnDestroy()
        {
            if (I == this) I = null;
        }


        public void PlayCue(AudioCueSO cue, Vector3 pos)
        {
            if (cue == null) return;
            if (cue.evt.IsNull) return;

            var t = Time.unscaledTime;
            if (cue.cooldown > 0f && t < cue.nextPlayableTime) return;
            cue.nextPlayableTime = t + cue.cooldown;

            cue.EnsureSampleDataLoaded();

            if (cue.is2D) RuntimeManager.PlayOneShot(cue.evt);
            else RuntimeManager.PlayOneShot(cue.evt, pos);
        }

        public static void PlayCue2(AudioCueSO cue, Vector3 pos)
        {
            if(I != null)
            {
                I.PlayCue(cue, pos);
            }
        }

        public EventInstance CreateAttachedInstance(AudioCueSO cue, Transform target, bool start)
        {
            if (cue == null) return default;
            if (cue.evt.IsNull) return default;

            cue.EnsureSampleDataLoaded();

            var inst = RuntimeManager.CreateInstance(cue.evt);

            // FIX: Rigidbody sitter ofta på parent (Player root)
            var rb = target.GetComponentInParent<Rigidbody>();
            RuntimeManager.AttachInstanceToGameObject(inst, target.gameObject, rb);

            if (start) inst.start();
            return inst;
        }

        public void StopAndRelease(ref EventInstance inst, bool fade)
        {
            if (!inst.isValid()) return;
            inst.stop(fade ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
            inst.release();
            inst.clearHandle();
        }
    }
}
