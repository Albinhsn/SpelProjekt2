using System;
using JetBrains.Annotations;
using srUtils.Unity;
using UnityEngine;
using UnityEngine.Events;

namespace Interaction
{
    [RequireComponent(typeof(Collider))]
    public class Interactable : MonoBehaviour
    {
        [SerializeField] protected InstanceSet m_interactableSet;
        [SerializeField] protected bool m_active = true;
        [SerializeField] protected bool m_requireUninteract = false;
        [SerializeField] protected bool m_canControlWhileInteracting = false;
        [SerializeField] protected bool m_cancelable = false;
        [SerializeField] protected UnityEvent<Interactor> m_onInteraction;
        [SerializeField] protected UnityEvent<Interactor> m_onInteractionCancelled;
        [SerializeField] protected UnityEvent<bool> m_toggleHighlighted;
        
        public Vector3 position => transform.position;

        public bool requireUninteract => m_requireUninteract;
        public bool canControlWhileInteracting => m_canControlWhileInteracting;
        [HideInInspector] public IndicatorKind m_indicatorKind;

        [CanBeNull] protected Interactor m_activeInteractor = null;


        private void Start()
        {
            VirtOnEnable();
        }
        
        protected virtual void VirtOnEnable()
        {
            if (m_active) m_interactableSet.Add(this);
        }
        private void OnDisable()
        {
            VirtOnDisable();
        }

        private void OnDestroy()
        {
            VirtOnDisable();
        }
        protected virtual void VirtOnDisable()
        {
            m_interactableSet.Remove(this);
        }

        public virtual void SendIndicatorRequest()
        {
        }


        public virtual void Interact(Interactor interactor)
        { 
            m_onInteraction.Invoke(interactor);
            m_activeInteractor = interactor;
        }

        public virtual bool TryCancelInteraction()
        {
            if (!m_cancelable) return false;
            if (m_activeInteractor is not null)
            {
                m_onInteractionCancelled.Invoke(m_activeInteractor);
            }
            m_activeInteractor = null;
            return true;
        }
        public virtual void ForceCancelInteraction()//May break external components that don't support interaction cancel
        {
            if (m_activeInteractor is not null)
            {
                m_onInteractionCancelled.Invoke(m_activeInteractor);
                m_activeInteractor.FinishInteraction();
            }
            m_activeInteractor = null;
        }

        public virtual void SetHighlighted(bool highlight) => m_toggleHighlighted.Invoke(highlight);

        public virtual void SetIsInteractable(bool active)
        {
            if (active == m_active) return;
            m_active = active;
            if (m_active) m_interactableSet.Add(this);
            else m_interactableSet.Remove(this);
        }
    }
}
