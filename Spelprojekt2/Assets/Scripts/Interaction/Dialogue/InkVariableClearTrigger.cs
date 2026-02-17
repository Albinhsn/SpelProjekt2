using System;
using UnityEngine;

namespace Interaction.Dialogue
{
    [RequireComponent(typeof(Collider))]
    public class InkVariableClearTrigger : MonoBehaviour
    {
        [SerializeField] private bool m_clearAll;
        [Tooltip("If not clearing all, clears these (if they have already been registered)")][SerializeField] private string[] m_toClear;
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if(m_clearAll) GlobalInkVariableManager.ClearAll();
                else GlobalInkVariableManager.ClearSelected(m_toClear);
            }
        }
    }
}