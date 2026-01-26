using UnityEngine;
using UnityEngine.Events;

namespace srUtils.Unity
{
    public abstract class OverlapDetector : MonoBehaviour 
    {
        [SerializeField] private UnityEvent onCollision;
        [SerializeField] private bool singleUse;
        public abstract bool Overlaps(Vector2 position, float radius);
        public void NotifyCollision()
        {
            if (singleUse) GetComponent<OverlapDetectorRegistrator>().DeactivateImmediate(this);
            onCollision.Invoke();
        }
    }
}