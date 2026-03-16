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
        [SerializeField] private float m_snapThreshold = 0.05f;
        [SerializeField] private float m_capsuleHeight = 1.5f;
        [SerializeField] private float m_holdRangeCorrectionStrength = 3;
        private float m_forwardLinearArc = 0.1f;
        private float m_maxBoundsSize;
        
        private float m_currentVelocity;

        private Vector3 m_upwardAimDirection => new Vector3(m_activeInteractor.aimDirection.x, MathF.Max(0, m_activeInteractor.aimDirection.y), m_activeInteractor.aimDirection.z).normalized;
        private Vector3 m_targetPosition => m_activeInteractor.position + m_upwardAimDirection * m_holdDistance;
        private Vector3 m_linearTargetDirection => (m_targetPosition - transform.position).normalized;
        private float m_targetDistance => Vector3.Distance(transform.position, m_targetPosition);
        private Vector3 m_interactorDirection => (m_activeInteractor.position - transform.position).normalized;
        private Vector3 m_closestLocalPoint => transform.position - m_activeInteractor.position; //m_rb.ClosestPointOnBounds(m_activeInteractor.position) - m_activeInteractor.position;

        private float m_distanceToInteractor => -m_capsuleHeight < m_closestLocalPoint.y && m_closestLocalPoint.y < 0
            ? new Vector2(m_closestLocalPoint.x, m_closestLocalPoint.z).magnitude
            : MathF.Min(m_closestLocalPoint.magnitude, (m_closestLocalPoint + new Vector3(0, -m_capsuleHeight, 0)).magnitude); 
        private float m_outerDistance => m_distanceToInteractor - m_holdDistance;
        private float m_decThreshold => MathF.Pow(m_rb.linearVelocity.magnitude, 2) / (2 * m_deceleration);
        private Vector3 m_sphericalTargetDirection => (m_linearTargetDirection - Vector3.Dot(m_interactorDirection, m_linearTargetDirection) * m_interactorDirection).normalized;
        private float m_forwardAngleCorrespondence => Vector3.Dot(m_activeInteractor.aimDirection, (transform.position - m_activeInteractor.position).normalized);

        private Vector3 m_holdRangeCorrection => new Vector3(-m_interactorDirection.x, MathF.Max(-m_interactorDirection.y, 0), -m_interactorDirection.z);


        private Rigidbody m_rb;
        private Rigidbody m_playerRb;
        private bool m_isHeld;

        private void Awake()
        {
            m_rb = GetComponent<Rigidbody>();
            m_maxBoundsSize = GetComponent<Collider>().bounds.extents.magnitude;
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
        public override void Interact(Interactor interactor)
        {
            base.Interact(interactor);
            m_savedCollissionDetectionMode = m_rb.collisionDetectionMode;
            m_rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            m_isHeld = true;
            m_rb.constraints |= RigidbodyConstraints.FreezeRotation;
            SfxDirector.PlayCue2(m_pickupCue, transform.position);
            m_playerRb = interactor.mainCollider.attachedRigidbody;
        }

        public override bool TryCancelInteraction()
        {
            m_rb.collisionDetectionMode = m_savedCollissionDetectionMode;
            m_isHeld = false;
            m_rb.constraints ^= RigidbodyConstraints.FreezeRotation;
            base.TryCancelInteraction();

            return true;
        }

        public override void ForceCancelInteraction()
        {
            m_rb.collisionDetectionMode = m_savedCollissionDetectionMode;
            m_isHeld = false;
            m_rb.constraints ^= RigidbodyConstraints.FreezeRotation;
            base.ForceCancelInteraction();
        }

        void Update()
        {
            if(m_isHeld)
            {
                PickupItemIndicatorManager.Request(this.transform.position, IndicatorKind.Held);
            }
        }
        
        private void FixedUpdate()
        {
            if (m_isHeld)
            {
                //Update velocity
                m_currentVelocity = m_targetDistance > m_decThreshold ? MathF.Min(m_maxVelocity, m_rb.linearVelocity.magnitude + m_acceleration * Time.fixedDeltaTime) : MathF.Max(0, m_currentVelocity - m_deceleration * Time.fixedDeltaTime);
                
                if (m_targetDistance < m_snapThreshold + (m_rb.linearVelocity.magnitude + m_acceleration * Time.fixedDeltaTime) * Time.fixedDeltaTime)//Snap, ignore deceleration
                {
                    m_currentVelocity = m_targetDistance / Time.fixedDeltaTime;
                    m_rb.linearVelocity = m_linearTargetDirection * m_currentVelocity;
                    return;
                }
                
                if (m_forwardAngleCorrespondence > 1 - m_forwardLinearArc && m_targetDistance < m_distanceToInteractor) //linear movement in forward cone
                {
                    m_rb.linearVelocity = m_linearTargetDirection * m_currentVelocity;
                }
                else
                {
                    
                    Vector3 selected_direction = (
                        
                            Vector3.Slerp(m_sphericalTargetDirection, m_linearTargetDirection, //Desired direction
                                  MathF.Min(1, //Spherical v linear movement T, clamp to max 1
                                  MathF.Max((m_forwardAngleCorrespondence - (1 - m_forwardLinearArc)) / m_forwardLinearArc, //Linear movement cone 
                                  MathF.Max(0, m_outerDistance / m_linearMovementThreshold //Linear movement outside of spherical movement range
                                  ))))
                    ).normalized;

                    if (Physics.Raycast(transform.position, selected_direction, out RaycastHit hit_info, m_maxBoundsSize + m_currentVelocity * Time.fixedDeltaTime, 1, QueryTriggerInteraction.Ignore))
                    {
                        Debug.Log("override");
                        Physics.Raycast(m_activeInteractor.position, m_upwardAimDirection, out hit_info, m_holdDistance + m_maxBoundsSize, 1, QueryTriggerInteraction.Ignore);
                        m_debugPoint = hit_info.point;
                        selected_direction = (hit_info.point - m_upwardAimDirection * m_maxBoundsSize - transform.position).normalized;
                        if (m_activeInteractor.mainCollider.Raycast(new Ray(transform.position, selected_direction), out hit_info, m_maxBoundsSize + m_currentVelocity * Time.fixedDeltaTime))
                        {
                            m_rb.linearVelocity = m_playerRb.linearVelocity;
                            return;
                        } 
                    }
                    else
                    {
                        Debug.Log("no override");
                        m_debugPoint = Vector3.zero;
                        selected_direction = Vector3.Slerp(selected_direction,
                            m_holdRangeCorrection, //Hold range correction
                            MathF.Max(0,
                                MathF.Min(1, (0 - m_outerDistance) / m_holdDistance * m_holdRangeCorrectionStrength)));//Hold range correction T
                    }

                    
                    
                    
                    
                    
                    m_rb.linearVelocity = m_playerRb.linearVelocity + selected_direction * m_currentVelocity;//Velocity scale
                }
                
                //TODO: fix oscillation
                
                //TODO: drop item if distance too great or it is behind wall
            }
        }

        private Vector3 m_debugPoint;

        private void OnDrawGizmos()
        {
            if(!m_isHeld) return;
            
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(m_debugPoint, m_snapThreshold); //Object target
            
            Gizmos.color = Color.red;
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

            
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + m_linearTargetDirection); //Linear direction
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + m_sphericalTargetDirection); //Spherical direction

            
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + (
                Vector3.Slerp(
                    Vector3.Slerp(m_sphericalTargetDirection, m_linearTargetDirection, //Desired direction
                        MathF.Min(1, //Spherical v linear movement T
                            MathF.Max(m_forwardAngleCorrespondence / (1 - m_forwardLinearArc), //Linear movement cone 
                                MathF.Max(0, m_outerDistance / m_linearMovementThreshold //Linear movement outside of spherical movement range
                                )))),
                    -m_interactorDirection, //Hold range correction
                    MathF.Max(0, MathF.Min(1, (0 - m_outerDistance) / m_holdDistance * m_holdRangeCorrectionStrength)))//Hold range correction T
            ).normalized);  //Active direction
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, m_maxBoundsSize);
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
