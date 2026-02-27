using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Interaction
{
    [RequireComponent(typeof(DynamicRigidbody))]
    public class Holdable : Interactable
    {
        [SerializeField] private float m_holdDistance = 2.25f;
        [SerializeField] private float m_linearMovementThreshold = 4;
        [SerializeField] private float m_maxVelocity = 10;
        [SerializeField] private float m_acceleration = 30;
        [SerializeField] private float m_deceleration = 20;
        [SerializeField] private float m_snapThreshold = 0.05f;
        [SerializeField] private float m_capsuleHeight = 1.5f;
        [SerializeField] private float m_holdRangeCorrectionStrength = 3;
        private float m_forwardLinearArc = 0.1f;
        
        private float m_currentVelocity;
        private Vector3 m_targetPosition => m_activeInteractor.position + m_activeInteractor.aimDirection * m_holdDistance;
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


        private Rigidbody m_rb;
        
        private Interactor m_activeInteractor;
        private bool m_isHeld;

        private void Awake()
        {
            m_rb = GetComponent<Rigidbody>();
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

        private CollisionDetectionMode m_savedCollissionDetectionMode;
        public override void Interact(Interactor interactor)
        {
            //m_rb.useGravity = false;
            m_savedCollissionDetectionMode = m_rb.collisionDetectionMode;
            m_rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            m_activeInteractor = interactor;
            m_isHeld = true;
            m_currentVelocity = 0;
        }

        public override bool TryCancelInteract(Interactor interactor)
        {
            //m_rb.useGravity = true;
            m_rb.collisionDetectionMode = m_savedCollissionDetectionMode;
            m_activeInteractor = null;
            m_isHeld = false;
            return true;
        }

        private void FixedUpdate()
        {
            if (m_isHeld)
            {
                //Update velocity
                m_currentVelocity = m_targetDistance > m_decThreshold ? MathF.Min(m_maxVelocity, m_rb.linearVelocity.magnitude + m_acceleration * Time.fixedDeltaTime) : MathF.Max(0, m_currentVelocity - m_deceleration * Time.fixedDeltaTime);
                
                if (m_targetDistance < m_snapThreshold + m_currentVelocity * Time.fixedDeltaTime)//Snap
                {
                    m_currentVelocity = m_targetDistance / Time.fixedDeltaTime;
                    m_rb.linearVelocity = m_linearTargetDirection * m_currentVelocity;
                    return;
                }
                
                
                if (m_forwardAngleCorrespondence > 1 - m_forwardLinearArc && m_targetDistance < m_distanceToInteractor)
                {
                    m_rb.linearVelocity = m_linearTargetDirection * m_currentVelocity;
                }
                else
                {
                    m_rb.linearVelocity = (
                        Vector3.Slerp(
                            Vector3.Slerp(m_sphericalTargetDirection, m_linearTargetDirection, //Desired direction
                                MathF.Min(1, //Spherical v linear movement T
                                  MathF.Max((m_forwardAngleCorrespondence - (1 - m_forwardLinearArc)) / m_forwardLinearArc, //Linear movement cone 
                                  MathF.Max(0, m_outerDistance / m_linearMovementThreshold //Linear movement outside of spherical movement range
                                  )))),
                        -m_interactorDirection, //Hold range correction
                        MathF.Max(0, MathF.Min(1, (0 - m_outerDistance) / m_holdDistance * m_holdRangeCorrectionStrength)))//Hold range correction T
                    ).normalized; 

                    m_rb.linearVelocity *= m_currentVelocity;//Velocity scale
                }
                
                //Wall clip prevention (unnecessary, now using continuous collision detection while held
                /*RaycastHit[] hit = Physics.RaycastAll(transform.position, m_rb.linearVelocity.normalized,
                    m_currentVelocity * Time.deltaTime, srUtils.Unity.Utils.GetPhysicsLayerMask(gameObject.layer));
                for (int a = 0; a < hit.Length; a++)
                {
                    if (hit[a].rigidbody != m_rb) //Wall detected
                    {
                        
                        break;
                    }
                }*/
            }
        }

        private void OnDrawGizmos()
        {
            if(!m_isHeld) return;
            
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(m_targetPosition, m_snapThreshold); //Object target
            
            
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