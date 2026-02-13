using UnityEngine;

namespace srUtils.Unity
{
    public class ResetOnAwake : MonoBehaviour
    {
        
        [SerializeField] private ResettableScriptableObject[] toReset;
        
        private void Awake()
        {
            foreach (ResettableScriptableObject obj in toReset)
            {
                obj.Reset();
            }
            Destroy(this);
        }
        
    }
}