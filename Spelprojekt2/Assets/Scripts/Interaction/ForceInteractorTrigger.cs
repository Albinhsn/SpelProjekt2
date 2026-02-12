using System;
using UnityEngine;

namespace Interaction
{
	[RequireComponent(typeof(Collider))]
    public class ForceInteractorTrigger : Interactable
    {
	    [SerializeField] private bool m_oneShot = true;
	    private void OnTriggerEnter(Collider other)
	    {
		    if(!m_active) return;
		    if (other.TryGetComponent(out Interactor interactor))
		    {
			    interactor.Interact(this);
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