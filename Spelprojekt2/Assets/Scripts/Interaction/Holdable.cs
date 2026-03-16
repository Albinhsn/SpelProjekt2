using System;
using AudioKit.FMOD;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Interaction
{
    [RequireComponent(typeof(DynamicRigidbody))]
    public class Holdable : Interactable
    {

        [Header("Audio")]
        [SerializeField] private AudioCueSO m_pickupCue;
        
        [SerializeField] private float m_holdDistance = 2.25f;
        [SerializeField] private float m_linearMovementThreshold = 4;
        [SerializeField] private float m_maxVelocity = 10;
        [SerializeField] private float m_acceleration = 30;
        [SerializeField] private float m_deceleration = 20;
        [SerializeField] private float m_capsuleHeight = 1.5f;
        [SerializeField] private float m_holdRangeCorrectionStrength = 3;
        
        private float m_antiOscillationThreshold = 0.1f;
        private float m_forwardLinearArc = 0.1f;
        private float m_holdCancelTime = 1.5f;
        
        private float m_boundingRadius;
        
        private float m_currentVelocity;

        private Vector3 m_horizontalAimDirection => new Vector3(m_activeInteractor.aimDirection.x, 0, m_activeInteractor.aimDirection.z).normalized;
        private Vector3 m_upwardAimDirection => m_activeInteractor.aimDirection.y > 0 ? m_activeInteractor.aimDirection : m_horizontalAimDirection;
        private Vector3 m_targetPosition => m_activeInteractor.position + m_upwardAimDirection * m_holdDistance;
        private Vector3 m_linearTargetDirection => (m_targetPosition - transform.position).normalized;
        private float m_targetDistance => Vector3.Distance(transform.position, m_targetPosition);
        private Vector3 m_interactorDirection => (m_activeInteractor.position - transform.position).normalized;
        private Vector3 m_localPosition => transform.position - m_activeInteractor.position; //m_rb.ClosestPointOnBounds(m_activeInteractor.position) - m_activeInteractor.position;

        private float m_distanceToInteractor => m_localPosition.magnitude - m_boundingRadius; /*-m_capsuleHeight < m_closestLocalPoint.y && m_closestLocalPoint.y < 0
            ? new Vector2(m_closestLocalPoint.x, m_closestLocalPoint.z).magnitude
            : MathF.Min(m_closestLocalPoint.magnitude, (m_closestLocalPoint + new Vector3(0, -m_capsuleHeight, 0)).magnitude - m_boundingRadius); */
        private float m_outerDistance => m_distanceToInteractor - m_holdDistance;
        private float m_decThreshold => MathF.Pow(m_rb.linearVelocity.magnitude, 2) / (2 * m_deceleration);
        private Vector3 m_sphericalTargetDirection => (m_linearTargetDirection - Vector3.Dot(m_interactorDirection, m_linearTargetDirection) * m_interactorDirection).normalized;
        private float m_forwardAngleCorrespondence => Vector3.Dot(m_upwardAimDirection, (transform.position - m_activeInteractor.position).normalized);

        private Vector3 m_holdRangeCorrection => new Vector3(-m_interactorDirection.x, MathF.Max(-m_interactorDirection.y, 0), -m_interactorDirection.z);


        private Rigidbody m_rb;
        private Rigidbody m_playerRb;
        private bool m_isHeld;

        private void Awake()
        {
            m_rb = GetComponent<Rigidbody>();
            m_boundingRadius = GetComponent<Collider>().bounds.extents.magnitude;
        }

        protected override void VirtOnDisable()
        {
            if (m_isHeld)
            {
                m_activeInteractor.FinishInteraction();
                m_rb.collisionDetectionMode = m_savedCollissionDetectionMode;
                m_activeInteractor = null;
                m_isHeld = false;
            }
            base.VirtOnDisable();
        }

        public override void SendIndicatorRequest()
        {
            if(!m_isHeld && m_indicatorKind != IndicatorKind.None)
            {
                PickupItemIndicatorManager.Request(this.transform.position, m_indicatorKind);
            }
        }

        private CollisionDetectionMode m_savedCollissionDetectionMode;
        private bool m_savedGravity;
        public override void Interact(Interactor interactor)
        {
            base.Interact(interactor);
            m_savedCollissionDetectionMode = m_rb.collisionDetectionMode;
            m_rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            m_isHeld = true;
            m_rb.constraints |= RigidbodyConstraints.FreezeRotation;
            SfxDirector.PlayCue2(m_pickupCue, transform.position);
            m_playerRb = interactor.mainCollider.attachedRigidbody;

            m_savedGravity = m_rb.useGravity;
            m_rb.useGravity = false;
        }

        public override bool TryCancelInteraction()
        {
            m_rb.collisionDetectionMode = m_savedCollissionDetectionMode;
            m_isHeld = false;
            m_rb.constraints ^= RigidbodyConstraints.FreezeRotation;
            base.TryCancelInteraction();
            m_rb.useGravity = m_savedGravity;
            return true;
        }

        public override void ForceCancelInteraction()
        {
            m_rb.collisionDetectionMode = m_savedCollissionDetectionMode;
            m_isHeld = false;
            m_rb.constraints ^= RigidbodyConstraints.FreezeRotation;
            m_rb.useGravity = m_savedGravity;
            base.ForceCancelInteraction();
        }

        void Update()
        {
            if(m_isHeld)
            {
                PickupItemIndicatorManager.Request(this.transform.position, IndicatorKind.Held);
            }
        }

        private float m_remainingHoldCancelTime;
        private void FixedUpdate()
        {
            if (m_isHeld)
            {
                //Auto drop logic
                if (m_distanceToInteractor > m_holdDistance * 2)
                {
                    ForceCancelInteraction();
                    return;
                }

                Physics.Raycast(m_activeInteractor.position, m_localPosition, out RaycastHit hit_info, m_localPosition.magnitude, m_activeInteractor.blockLineOfSight);
                if (hit_info.transform != transform)
                {
                    m_remainingHoldCancelTime -= Time.fixedDeltaTime;
                    if (m_remainingHoldCancelTime < 0)
                    {
                        ForceCancelInteraction();
                        return;
                    }
                }
                else m_remainingHoldCancelTime = m_holdCancelTime;
                
                
                //Update velocity
                m_currentVelocity = m_targetDistance > m_decThreshold ? MathF.Min(m_maxVelocity, m_rb.linearVelocity.magnitude + m_acceleration * Time.fixedDeltaTime) : MathF.Max(0, m_currentVelocity - m_deceleration * Time.fixedDeltaTime);
                
                Vector3 selected_direction = (
                    Vector3.Slerp(m_sphericalTargetDirection, m_linearTargetDirection, //Desired direction
                        MathF.Min(1, //Spherical v linear movement T, clamp to max 1
                            MathF.Max(MathF.Min(1, Vector3.Dot(m_localPosition.normalized, m_upwardAimDirection)), //Linearize in front of player
                                MathF.Max(0, m_outerDistance / m_linearMovementThreshold //Linear movement outside of spherical movement range
                                ))))
                ).normalized;
                    
                m_rb.linearVelocity = selected_direction * m_currentVelocity;//Velocity scale
                
                if (m_targetDistance < m_antiOscillationThreshold) m_rb.linearVelocity *= m_targetDistance / m_antiOscillationThreshold;
                //m_rb.linearVelocity += m_playerRb.linearVelocity * MathF.Min(1, 1 + Vector3.Dot(m_rb.linearVelocity.normalized, m_playerRb.linearVelocity.normalized));
                
                //Emergency player collision avoidance (flight prevention)
                float inner_collision_emergency_distance = (m_activeInteractor.mainCollider.ClosestPoint(transform.position) - transform.position).magnitude - m_boundingRadius + 0.1f;
                if (inner_collision_emergency_distance < 0) //Object will collide. Emergency
                {
                    Vector3 normalized_position = (transform.position - m_activeInteractor.transform.position);
                    m_rb.linearVelocity += (new Vector3(normalized_position.x, 0, normalized_position.z) * -inner_collision_emergency_distance) / Time.fixedDeltaTime;
                }
                
            }
        }


        private void OnDrawGizmos()
        {
            if(!m_isHeld) return;
            
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(m_targetPosition, m_antiOscillationThreshold); //Object target
            
            /*Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(m_activeInteractor.position, m_holdDistance); //Min distance on interactor
            Gizmos.DrawWireSphere(m_activeInteractor.position + Vector3.down * m_capsuleHeight, m_holdDistance); //Interactor capsule lower part
            Gizmos.DrawLine(m_activeInteractor.position + new Vector3(m_holdDistance, 0, 0), m_activeInteractor.position + new Vector3(m_holdDistance, -m_capsuleHeight, 0));//interactor capsule side lines
            Gizmos.DrawLine(m_activeInteractor.position + new Vector3(-m_holdDistance, 0, 0), m_activeInteractor.position + new Vector3(-m_holdDistance, -m_capsuleHeight, 0));
            Gizmos.DrawLine(m_activeInteractor.position + new Vector3(0, 0, m_holdDistance), m_activeInteractor.position + new Vector3(0, -m_capsuleHeight, m_holdDistance));
            Gizmos.DrawLine(m_activeInteractor.position + new Vector3(0, 0, -m_holdDistance), m_activeInteractor.position + new Vector3(0, -m_capsuleHeight, -m_holdDistance));
            
            Gizmos.DrawWireSphere(transform.position, m_holdDistance); //Min distance on object
            Gizmos.DrawWireSphere(transform.position + Vector3.up * m_capsuleHeight, m_holdDistance); //Capsule upper part
            Gizmos.DrawLine(transform.position + new Vector3(m_holdDistance, 0, 0), transform.position + new Vector3(m_holdDistance, m_capsuleHeight, 0));//Capsule side lines
            Gizmos.DrawLine(transform.position + new Vector3(-m_holdDistance, 0, 0), transform.position + new Vector3(-m_holdDistance, m_capsuleHeight, 0));
            Gizmos.DrawLine(transform.position + new Vector3(0, 0, m_holdDistance), transform.position + new Vector3(0, m_capsuleHeight, m_holdDistance));
            Gizmos.DrawLine(transform.position + new Vector3(0, 0, -m_holdDistance), transform.position + new Vector3(0, m_capsuleHeight, -m_holdDistance));
            
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(m_activeInteractor.position, m_linearMovementThreshold); //Linear movement threshold
            */
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + m_linearTargetDirection); //Linear direction
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + m_sphericalTargetDirection); //Spherical direction

            
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + m_rb.linearVelocity.normalized);//Active direction
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, m_boundingRadius);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, m_holdDistance); //Min distance on object
            Gizmos.DrawWireSphere(transform.position + Vector3.up * m_capsuleHeight, m_holdDistance); //Capsule upper part
            Gizmos.DrawLine(transform.position + new Vector3(m_holdDistance, 0, 0), transform.position + new Vector3(m_holdDistance, m_capsuleHeight, 0));//Capsule side lines
            Gizmos.DrawLine(transform.position + new Vector3(-m_holdDistance, 0, 0), transform.position + new Vector3(-m_holdDistance, m_capsuleHeight, 0));
            Gizmos.DrawLine(transform.position + new Vector3(0, 0, m_holdDistance), transform.position + new Vector3(0, m_capsuleHeight, m_holdDistance));
            Gizmos.DrawLine(transform.position + new Vector3(0, 0, -m_holdDistance), transform.position + new Vector3(0, m_capsuleHeight, -m_holdDistance));
        }
    }
}
