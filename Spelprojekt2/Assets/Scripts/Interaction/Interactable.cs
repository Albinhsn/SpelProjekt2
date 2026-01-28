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
        [SerializeField] private UnityEvent<Interactor> m_onInteraction;
        [SerializeField] private UnityEvent<bool> m_highlight;
        
        public Vector3 position => transform.position;


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
            Debug.Log($"Interacted with {gameObject.name}");
            m_onInteraction.Invoke(interactor);
        }

        public void SetHighlighted(bool highlight) => m_highlight.Invoke(highlight);

        public void SetActive(bool active)
        {
            if (active == m_active) return;
            m_active = active;
            if (m_active) m_interactableSet.Add(this);
            else m_interactableSet.Remove(this);
        }
    }
}