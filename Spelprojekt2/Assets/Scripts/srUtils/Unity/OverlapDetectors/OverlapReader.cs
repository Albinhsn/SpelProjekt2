using UnityEngine;

namespace srUtils.Unity
{
    public class OverlapReader : MonoBehaviour
    {
        [SerializeField] private InstanceSet colliderSet;

        public bool Overlaps(float radius)
        {
            foreach (OverlapDetector obj in colliderSet)
            {
                if (obj.Overlaps(transform.position, radius)) return true;
            }

            return false;
        }
    }
}