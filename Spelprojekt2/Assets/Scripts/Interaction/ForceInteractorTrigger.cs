using System;
using UnityEngine;

namespace Interaction
{
	[RequireComponent(typeof(Collider), typeof(SerializableObject))]
    public class ForceInteractorTrigger : Interactable
    {
        public bool m_hasBeenActivated => !base.m_active;

        public void SetActive(bool active)
        {
            base.m_active = active;
        }

	    [SerializeField] private bool m_oneShot = true;
	    private void OnTriggerEnter(Collider other)
	    {
		    if(!m_active) return;
		    Interactor interactor = other.GetComponentInChildren<Interactor>();
		    if (interactor is not null)
		    {
			    interactor.CancelInteractions();
			    interactor.Interact(this, 0);
		    }
	    }

	    public override void Interact(Interactor interactor)
	    {
		    base.Interact(interactor);
		    base.m_active = !m_oneShot;
	    }

	    protected override void VirtOnDisable()
	    {
	    }
	    protected override void VirtOnEnable()
	    {
	    }
    }
}
