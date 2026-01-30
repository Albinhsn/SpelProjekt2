using System;
using System.Collections.Generic;
using UnityEngine;

// AudioKit anteckning
// Footsteps system
// Surface på marken bestämmer vilka ljud
// Controller kan vara auto eller animation events


namespace AudioKit.FMOD
{
    public enum SurfaceType
    {
        Default = 0,
        Concrete = 1,
        Metal = 2,
        Wood = 3,
        Grass = 4,
        Dirt = 5
    }

    [Serializable]
    public struct FootstepSet
    {
        public SurfaceType surface;
        public AudioCueSO walk;
        public AudioCueSO run;
        public AudioCueSO jump;
        public AudioCueSO land;
    }

    [CreateAssetMenu(menuName = "AudioKit/Ljud/FootstepLibrary", fileName = "FootstepLibrary")]
    public sealed class FootstepLibrarySO : ScriptableObject
    {
        public FootstepSet[] sets = Array.Empty<FootstepSet>();

        private Dictionary<SurfaceType, FootstepSet> map;

        private void OnEnable()
        {
            map = new Dictionary<SurfaceType, FootstepSet>(sets.Length);
            for (int i = 0; i < sets.Length; i++)
            {
                map[sets[i].surface] = sets[i];
            }
        }

        public bool TryGet(SurfaceType surface, out FootstepSet set)
        {
            if (map == null) OnEnable();
            return map.TryGetValue(surface, out set);
        }
    }

    [DisallowMultipleComponent]
    public sealed class FootstepSurface : MonoBehaviour
    {
        public SurfaceType surface = SurfaceType.Default;
    }

    [DisallowMultipleComponent]
    public sealed class FootstepController : MonoBehaviour
    {
        [Header("Läge")]
        [SerializeField] private bool autoDistance = true;

        [Header("Auto steg")]
        [SerializeField] private float stepDistanceWalk = 1.8f;
        [SerializeField] private float stepDistanceRun = 1.2f;
        [SerializeField] private float runSpeed = 4.5f;

        [Header("Marktest")]
        [SerializeField] private float rayLength = 1.6f;
        [SerializeField] private LayerMask groundMask = ~0;

        private Vector3 lastPos;
        private float accum;

        private Rigidbody rb;
        private CharacterController cc;

        private void Awake()
        {
            rb = GetComponentInParent<Rigidbody>();
            cc = GetComponentInParent<CharacterController>();
            lastPos = transform.position;
        }

        private void Update()
        {
            if (!autoDistance) return;
            if (SfxDirector.I == null) return;

            var pos = transform.position;
            var delta = pos - lastPos;
            lastPos = pos;

            var speed = GetSpeed(delta);
            if (speed < 0.1f) return;

            accum += delta.magnitude;

            var stepDist = (speed >= runSpeed) ? stepDistanceRun : stepDistanceWalk;
            if (accum >= stepDist)
            {
                accum = 0f;
                PlayFootstep(speed >= runSpeed);
            }
        }

        private float GetSpeed(Vector3 delta)
        {
            if (rb != null) return rb.linearVelocity.magnitude;
            if (cc != null) return cc.velocity.magnitude;
            return delta.magnitude / Mathf.Max(0.0001f, Time.deltaTime);
        }

        private void PlayFootstep(bool run)
        {
            var lib = AudioResources.Footsteps;
            if (lib == null) return;

            var surface = ResolveSurface();

            if (!lib.TryGet(surface, out var set))
                lib.TryGet(SurfaceType.Default, out set);

            var cue = run ? set.run : set.walk;
            if (cue == null) cue = set.walk;

            if (cue != null)
                SfxDirector.I.PlayCue(cue, transform.position);
        }

        private SurfaceType ResolveSurface()
        {
            if (Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out var hit, rayLength, groundMask, QueryTriggerInteraction.Ignore))
            {
                var s = hit.collider.GetComponentInParent<FootstepSurface>();
                if (s != null) return s.surface;
            }
            return SurfaceType.Default;
        }

        public void AnimEvent_FootstepWalk() => PlayFootstep(false);
        public void AnimEvent_FootstepRun() => PlayFootstep(true);
    }
}
