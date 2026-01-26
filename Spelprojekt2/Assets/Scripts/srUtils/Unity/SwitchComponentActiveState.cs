using UnityEngine;

namespace srUtils.Unity
{
    public class SwitchComponentActiveState : MonoBehaviour
    {
        [SerializeField] private Behaviour[] components;

        public void Switch()
        {
            foreach (Behaviour obj in components)
            {
                obj.enabled = !obj.enabled;
            }
        }
    }
}