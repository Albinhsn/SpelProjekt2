using UnityEngine;

namespace SP2.Audio
{
    // Två sätt:
    // 1) Auto (distance): ackumulerar distans när spelaren rör sig
    // 2) Animation events: kalla FootstepLeft/Right från animationer
    [DisallowMultipleComponent]
    public sealed class FootstepController : MonoBehaviour
    {
        public enum Mode
        {
            AutoDistance = 0,
            AnimationEvents = 1
        }

        [Header("== Mode ==")]
        public Mode mode = Mode.AutoDistance;

        [Header("== Library ==")]
        [Tooltip("Valfritt: om tomt försöker systemet ladda Resources/Audio/FootstepLibrary.asset")] 
        public FootstepLibrarySO library;

        [Header("== Emit position ==")]
        [Tooltip("Raycast start + fallback för spelposition. Om tomt används transform.")]
        public Transform origin;

        [Tooltip("Valfritt: om satt kan du spela vänster/höger fot separat via animation events.")]
        public Transform leftFoot;
        public Transform rightFoot;
        public bool alternateFeet = true;

        [Header("== Ground detection ==")]
        public LayerMask groundMask = ~0;
        [Min(0.05f)] public float groundRayDistance = 0.35f;
        [Tooltip("Extra offset uppåt för raycast-start (för att inte starta inuti mark).")]
        public float rayStartOffset = 0.1f;

        [Header("== Auto-distance stepping ==")]
        [Min(0f)] public float minSpeed = 0.1f;
        [Min(0.1f)] public float runSpeedThreshold = 4.0f;
        [Min(0.1f)] public float stepDistanceWalk = 1.6f;
        [Min(0.1f)] public float stepDistanceRun = 1.1f;

        [Header("== Auto jump/land (valfritt) ==")]
        public bool autoJumpLand = true;

        private CharacterController _cc;
        private Rigidbody _rb;

        private Vector3 _lastPos;
        private float _accumulated;
        private bool _wasGrounded;
        private bool _nextLeft = true;

        private void Awake()
        {
            if (origin == null)
                origin = transform;

            _cc = GetComponent<CharacterController>();
            _rb = GetComponent<Rigidbody>();
            _lastPos = origin.position;
            _wasGrounded = IsGrounded(out _);
        }

        private void Update()
        {
            if (mode != Mode.AutoDistance)
                return;

            Vector3 pos = origin.position;
            Vector3 delta = pos - _lastPos;
            _lastPos = pos;

            // Bara horizontal distans
            delta.y = 0f;

            float dt = Mathf.Max(Time.deltaTime, 0.0001f);
            float speed = GetSpeed(delta, dt);

            bool grounded = IsGrounded(out var hit);
            if (autoJumpLand)
            {
                if (_wasGrounded && !grounded)
                    JumpInternal(hit);
                else if (!_wasGrounded && grounded)
                    LandInternal(hit);
            }
            _wasGrounded = grounded;

            if (!grounded)
            {
                _accumulated = 0f;
                return;
            }

            if (speed < minSpeed)
            {
                _accumulated = 0f;
                return;
            }

            _accumulated += delta.magnitude;

            bool running = speed >= runSpeedThreshold;
            float stepDist = running ? stepDistanceRun : stepDistanceWalk;

            if (_accumulated >= stepDist)
            {
                _accumulated = 0f;
                StepInternal(hit, running);
            }
        }

        public void FootstepLeft()  => StepInternal(GetGroundHit(out var hit) ? hit : default, false, true);
        public void FootstepRight() => StepInternal(GetGroundHit(out var hit) ? hit : default, false, false);

        // Generic event (om du bara vill kalla en funktion)
        public void Footstep() => StepInternal(GetGroundHit(out var hit) ? hit : default, false);

        public void Jump() => JumpInternal(GetGroundHit(out var hit) ? hit : default);
        public void Land() => LandInternal(GetGroundHit(out var hit) ? hit : default);

        private void StepInternal(RaycastHit hit, bool running, bool? forceLeft = null)
        {
            var lib = ResolveLibrary();
            if (lib == null) return;
            if (AudioSystem.Instance == null || AudioSystem.Instance.Sfx == null) return;

            SurfaceType surface = FootstepResolver.ResolveSurfaceFromHit(hit);
            var cue = lib.GetStepCue(surface, running);
            if (cue == null) return;

            Vector3 emitPos = ResolveEmitPosition(forceLeft);
            AudioSystem.Instance.Sfx.Play(cue, emitPos);
        }

        private void JumpInternal(RaycastHit hit)
        {
            var lib = ResolveLibrary();
            if (lib == null) return;
            if (AudioSystem.Instance == null || AudioSystem.Instance.Sfx == null) return;

            SurfaceType surface = FootstepResolver.ResolveSurfaceFromHit(hit);
            var cue = lib.GetJumpCue(surface);
            if (cue == null) return;

            AudioSystem.Instance.Sfx.Play(cue, ResolveEmitPosition(null));
        }

        private void LandInternal(RaycastHit hit)
        {
            var lib = ResolveLibrary();
            if (lib == null) return;
            if (AudioSystem.Instance == null || AudioSystem.Instance.Sfx == null) return;

            SurfaceType surface = FootstepResolver.ResolveSurfaceFromHit(hit);
            var cue = lib.GetLandCue(surface);
            if (cue == null) return;

            AudioSystem.Instance.Sfx.Play(cue, ResolveEmitPosition(null));
        }

        private FootstepLibrarySO ResolveLibrary()
        {
            if (library != null)
                return library;

            // Auto: ladda från Resources
            return AudioResources.Footsteps;
        }

        private Vector3 ResolveEmitPosition(bool? forceLeft)
        {
            if (leftFoot == null || rightFoot == null)
                return origin != null ? origin.position : transform.position;

            bool left = forceLeft ?? (alternateFeet ? _nextLeft : true);
            if (alternateFeet && forceLeft == null)
                _nextLeft = !_nextLeft;

            return left ? leftFoot.position : rightFoot.position;
        }

        private float GetSpeed(float dt)
        {
            if (_cc != null)
            {
                var v = _cc.velocity;
                v.y = 0f;
                return v.magnitude;
            }

            if (_rb != null)
            {
                var v = _rb.linearVelocity;
                v.y = 0f;
                return v.magnitude;
            }

            return 0f;
        }

        private float GetSpeed(Vector3 horizontalDelta, float dt)
        {
            float v = GetSpeed(dt);
            if (v > 0f) return v;
            return horizontalDelta.magnitude / dt;
        }

        private bool IsGrounded(out RaycastHit hit)
        {
            if (_cc != null)
            {
                // CharacterController.isGrounded kan ibland ligga efter, men funkar ofta bra.
                if (_cc.isGrounded)
                {
                    GetGroundHit(out hit);
                    return true;
                }
            }

            return GetGroundHit(out hit);
        }

        private bool GetGroundHit(out RaycastHit hit)
        {
            Vector3 start = (origin != null ? origin.position : transform.position) + Vector3.up * rayStartOffset;
            return Physics.Raycast(start, Vector3.down, out hit, groundRayDistance + rayStartOffset, groundMask, QueryTriggerInteraction.Ignore);
        }
    }
}
