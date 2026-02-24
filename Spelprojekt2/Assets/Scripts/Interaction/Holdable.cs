using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Interaction
{
    [RequireComponent(typeof(DynamicRigidbody))]
    public class Holdable : Interactable
    {
        [SerializeField] private float m_holdDistance;
        [SerializeField] private float m_linearMovementThreshold;
        [SerializeField] private float m_acceleration;
        [SerializeField] private float m_maxVelocity;
        [SerializeField] private float m_deceleration;
        [SerializeField] private float m_snapThreshold;
        
        private float m_currentVelocity;
        private Vector3 m_targetPosition => m_activeInteractor.position + m_activeInteractor.aimDirection * m_holdDistance;
        private Vector3 m_linearTargetDirection => (transform.position - m_targetPosition).normalized;
        private float m_targetDistance => Vector3.Distance(transform.position, m_targetPosition);
        private Vector3 m_interactorDirection => transform.position - m_activeInteractor.position;
        private float m_interactorDistance => Vector3.Distance(transform.position, m_activeInteractor.position);
        private float m_outerDistance => m_interactorDistance - m_holdDistance;
        private float m_decThreshold => MathF.Pow(m_currentVelocity, 2) / (2 * m_deceleration);
        private Vector3 m_sphericalTargetDirection => (m_linearTargetDirection - m_interactorDirection * Vector3.Dot(m_linearTargetDirection, m_interactorDirection)).normalized;
        
        
        
        private Interactor m_activeInteractor;
        private bool m_isHeld;
        public override void Interact(Interactor interactor)
        {
            m_activeInteractor = interactor;
            m_isHeld = true;
        }

        public override bool TryCancelInteract(Interactor interactor)
        {
            m_activeInteractor = null;
            m_isHeld = false;
            return true;
        }

        private void Update()
        {
            if (m_isHeld)
            {
                //Update velocity
                m_currentVelocity = m_targetDistance > m_decThreshold ? MathF.Min(m_maxVelocity, m_currentVelocity + m_acceleration) : m_currentVelocity - m_deceleration;
                
                if (m_outerDistance > m_linearMovementThreshold) //Move straight towards target outside some range
                {
                    transform.position += m_linearTargetDirection * (m_currentVelocity * Time.deltaTime);
                }
                else //Spherical movement
                {
                    transform.position += Vector3.Slerp(m_sphericalTargetDirection, m_linearTargetDirection,
                        MathF.Min(0, (m_interactorDistance - m_outerDistance) / m_linearMovementThreshold)) * (m_currentVelocity * Time.deltaTime);
                }
            }
        }
    }
}