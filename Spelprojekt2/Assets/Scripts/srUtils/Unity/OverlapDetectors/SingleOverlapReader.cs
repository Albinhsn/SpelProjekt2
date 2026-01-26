using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using srUtils.Unity.Events;
using UnityEngine;
using UnityEngine.Events;

namespace srUtils.Unity
{
    public class SingleOverlapReader : MonoBehaviour
    {
        [SerializeField] private InstanceSet colliderSet;
        [SerializeField] private float radius = 1;
        [SerializeField] private UnityEvent localEvent;
        [Tooltip("Can be null")][SerializeField] [CanBeNull] private EventAsset globalEvent;
        
        [SerializeField] private OverlapDetector[] ignoredDetectors;
        
        public void SetRadius(float radius)
        {
            this.radius = radius;
        }

        public void Trigger()
        {
            OverlapDetector[] overlapDetectors = colliderSet.GetEnumerable().Cast<OverlapDetector>().Where(e => !ignoredDetectors.Contains(e)).ToArray();
            for (int a = overlapDetectors.Length - 1; a >= 0; a--)
            {
                if(overlapDetectors[a].transform.parent==transform.parent)Debug.Log("Ignore didn't work");
                if (overlapDetectors[a].Overlaps(transform.position, radius))
                {
                    localEvent.Invoke();
                    globalEvent?.Invoke();
                    overlapDetectors[a].NotifyCollision();
                }
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}