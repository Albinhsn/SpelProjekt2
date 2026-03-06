using System;
using AudioKit.FMOD;
using JetBrains.Annotations;
using UnityEditor.UIElements;
using UnityEngine;

namespace Interaction
{
    [RequireComponent(typeof(DynamicRigidbody))]
    public class Holdable : Interactable
    {
        [Header("Indicator")]
        [SerializeField] private HeldItemIndicator m_indicatorPrefab;
        [SerializeField] private HeldItemIndicator m_indicator;

        [Header("Audio")]
        [SerializeField] private AudioCueSO m_pickupCue;
        
        [Header("Hold settings")]
        [SerializeField] private float m_holdDistance = 2.25f;
        [SerializeField] private float m_linearMovementThreshold = 4;
        [SerializeField] private float m_maxVelocity = 10;
        [SerializeField] private float m_acceleration = 30;
        [SerializeField] private float m_deceleration = 20;
        [SerializeField] private float m_snapThreshold = 0.05f;
        [SerializeField] private float m_capsuleHeight = 1.5f;
        [SerializeField] private float m_holdRangeCorrectionStrength = 3;
        [SerializeField] private float m_itemDistanceFromPlayer = 2;
        [SerializeField] private float m_itemFloatHeight = .75f;
        [SerializeField] private float m_itemMaxSpeed = 10;
        [SerializeField] private LayerMask m_PickedUpLayerMask;
        private LayerMask m_startLayer => gameObject.layer;
        private float m_forwardLinearArc = 0.1f;
        
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


        private Rigidbody m_rb;
        private bool m_isHeld;

        private void Awake()
        {
            m_rb              = GetComponent<Rigidbody>();
            m_indicatorPrefab = Resources.Load<HeldItemIndicator>("Prefabs/HeldItemIndicator");
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
            m_currentVelocity = 0;
            m_rb.constraints |= RigidbodyConstraints.FreezeRotation;
            // gameObject.layer = m_PickedUpLayerMask;            
            SfxDirector.PlayCue2(m_pickupCue, transform.position);

            if(m_indicatorPrefab != null)
            {
                m_indicator = Instantiate(m_indicatorPrefab, Vector3.zero, Quaternion.identity);
                m_indicator.Init(this.gameObject);
            }
        }

        public override bool TryCancelInteraction()
        {
            m_rb.collisionDetectionMode = m_savedCollissionDetectionMode;
            m_activeInteractor = null;
            m_isHeld = false;
            base.TryCancelInteraction();
            m_rb.constraints ^= RigidbodyConstraints.FreezeRotation;
            // gameObject.layer = m_startLayer;            

            if(m_indicator != null)
            {
                Destroy(m_indicator.gameObject);
            }

            return true;
        }

        public override void ForceCancelInteraction()
        {
            TryCancelInteraction();
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
                Vector3 targetPos = m_activeInteractor.transform.position + m_activeInteractor.transform.forward * m_itemDistanceFromPlayer + new Vector3(0, m_itemFloatHeight, 0);
                Vector3 itemToPlayerDir = m_activeInteractor.transform.position - transform.position;
                Vector2 itemToPlayerDirXZ = new Vector2(itemToPlayerDir.x, itemToPlayerDir.z).normalized;
                Vector2 itemToPlayerDirXZNormal = new Vector2(-itemToPlayerDirXZ.y, itemToPlayerDirXZ.x);
                float itemToPlayerDist = itemToPlayerDir.magnitude;
                float itemToPlayerDistXZ = new Vector2(itemToPlayerDir.x, itemToPlayerDir.z).magnitude;
                float distanceToCircle = itemToPlayerDist - m_itemDistanceFromPlayer;
                float distanceToCircleXZ = itemToPlayerDistXZ - m_itemDistanceFromPlayer;
                Vector3 itemToTargetPosDir = transform.position + (itemToPlayerDir * distanceToCircle);
                Vector3 directionToTargetFromCircle = targetPos - itemToTargetPosDir;
                float distanceToTargetFromItemXZ = new Vector2(targetPos.x - transform.position.x, targetPos.z - transform.position.z).magnitude;

                // if item is outside of the circle the value is negative, if it's inside, the value is positive
                float forwardDir = -Mathf.Sign(distanceToCircleXZ);
                float Dir = Mathf.Sign(Vector2.Dot(itemToPlayerDirXZNormal, new Vector2(directionToTargetFromCircle.x, directionToTargetFromCircle.z)));

                float angle = Mathf.Clamp(Mathf.Abs(distanceToCircleXZ) * 100f,0,45f);

                Vector3 targetVelocity = new Vector3(
                    itemToPlayerDirXZNormal.x * Mathf.Cos(Mathf.Deg2Rad*forwardDir*Dir*angle) - itemToPlayerDirXZNormal.y * Mathf.Sin(Mathf.Deg2Rad*forwardDir*Dir*angle),
                    targetPos.y - transform.position.y,
                    itemToPlayerDirXZNormal.x * Mathf.Sin(Mathf.Deg2Rad*forwardDir*Dir*angle) + itemToPlayerDirXZNormal.y * Mathf.Cos(Mathf.Deg2Rad*forwardDir*Dir*angle)
                );
    
                // targetVelocity = targetPos - transform.position;
                
                float speed = Mathf.Clamp(distanceToTargetFromItemXZ*distanceToTargetFromItemXZ, 0, m_itemMaxSpeed);
                float verticalSpeed = Mathf.Clamp(Mathf.Abs(targetPos.y - transform.position.y * targetPos.y - transform.position.y * 2), 0, m_itemMaxSpeed);
                Rigidbody rb = gameObject.GetComponent<Rigidbody>();
                if(rb != null)
                {
                    rb.linearVelocity = GameObject.FindFirstObjectByType<Player>().GetComponent<Rigidbody>().linearVelocity;
                    rb.linearVelocity += new Vector3(targetVelocity.x * Dir * speed, targetVelocity.y * verticalSpeed, targetVelocity.z * Dir * speed);
                    // rb.linearVelocity += targetVelocity.normalized;
                }
                Vector3 dir = m_activeInteractor.transform.position - transform.position;
                if(dir.magnitude > m_itemDistanceFromPlayer * 3f)
                {
                    TryCancelInteraction();
                }
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
