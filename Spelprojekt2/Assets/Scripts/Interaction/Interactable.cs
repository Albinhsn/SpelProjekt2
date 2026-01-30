using System;
using srUtils.Unity;
using UnityEngine;
using UnityEngine.Events;

namespace Interaction
{
    public class Interactable : MonoBehaviour
    {
        [SerializeField] private InstanceSet m_interactableSet;
        [SerializeField] private bool m_active = true;
        [SerializeField] private bool m_requireUninteract = false;
        [SerializeField] private UnityEvent<Interactor> m_onInteraction;
        [SerializeField] private UnityEvent<bool> m_toggleHighlighted;
        
        public Vector3 position => transform.position;

        public bool requireUninteract => m_requireUninteract;


        private void OnEnable()
        {
            if (m_active) m_interactableSet.Add(this);
        }
        private void OnDisable()
        {
            if (m_active) m_interactableSet.Remove(this);
        }

        
        public void Interact(Interactor interactor)
        {
            Debug.Log($"{interactor.gameObject.name} interacted with {gameObject.name}");
            m_onInteraction.Invoke(interactor);
        }

        public void SetHighlighted(bool highlight) => m_toggleHighlighted.Invoke(highlight);

        public void SetActive(bool active)
        {
            if (active == m_active) return;
            m_active = active;
            if (m_active) m_interactableSet.Add(this);
            else m_interactableSet.Remove(this);
        }
    }
}