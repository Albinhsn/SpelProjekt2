using UnityEngine;

namespace srUtils.Unity
{
    public class DeactivateComponents : MonoBehaviour
    {
        [SerializeField] private Behaviour[] components;

        public void Deactivate()
        {
            foreach (Behaviour obj in components)
            {
                obj.enabled = false;
            }
        }
    }
}