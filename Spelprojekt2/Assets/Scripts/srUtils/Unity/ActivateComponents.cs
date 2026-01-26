using UnityEngine;

namespace srUtils.Unity
{
    public class ActivateComponents : MonoBehaviour
    {
        [SerializeField] private Behaviour[] components;

        public void Activate()
        {
            foreach (Behaviour obj in components)
            {
                obj.enabled = true;
            }
        }
    }
}