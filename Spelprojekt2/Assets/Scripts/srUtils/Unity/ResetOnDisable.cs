using UnityEngine;

namespace srUtils.Unity
{
    public class ResetOnDisable : MonoBehaviour
    {
        
        [SerializeField] private ResettableScriptableObject[] toReset;
        
        private void OnDisable()
        {
            foreach (ResettableScriptableObject obj in toReset)
            {
                obj.Reset();
            }
            Destroy(this);
        }
        
    }
}