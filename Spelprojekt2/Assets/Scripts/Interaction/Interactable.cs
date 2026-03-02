using System;
using srUtils.Unity;
using UnityEngine;
using UnityEngine.Events;

namespace Interaction
{
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


        private void Start()
        {
            VirtOnEnable();
        }
        private void OnEnable()
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
            if (m_active) m_interactableSet.Remove(this);
        }


        public virtual void Interact(Interactor interactor)
        { 
            m_onInteraction.Invoke(interactor);
            Debug.Log($"interacting with {gameObject.name}");
        }

        public virtual bool TryCancelInteract(Interactor interactor)
        {
            m_onInteractionCancelled.Invoke(interactor);
            return m_cancelable;
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