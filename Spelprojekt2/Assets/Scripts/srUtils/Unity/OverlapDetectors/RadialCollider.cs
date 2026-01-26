using UnityEngine;

namespace srUtils.Unity
{
    public class RadialCollider : OverlapDetector
    {
        [SerializeField] private float radius = 1;
        public void SetRadius(float radius)
        {
            this.radius = radius;
        }

        public override bool Overlaps(Vector2 position, float radius)
        {
            return ((Vector2)transform.position - position).magnitude < this.radius + radius;
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}