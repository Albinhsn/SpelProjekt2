using UnityEngine;

namespace SP2.Audio
{
    public static class FootstepResolver
    {
        public static SurfaceType ResolveSurfaceFromHit(RaycastHit hit)
        {
            if (hit.collider == null)
                return SurfaceType.Default;

            // 1) Direkt på collider-objectet
            if (hit.collider.TryGetComponent<FootstepSurface>(out var fs))
                return fs.surfaceType;

            // 2) På parent (affectChildren)
            var parent = hit.collider.transform.parent;
            while (parent != null)
            {
                if (parent.TryGetComponent<FootstepSurface>(out var pfs) && pfs.affectChildren)
                    return pfs.surfaceType;
                parent = parent.parent;
            }

            return SurfaceType.Default;
        }
    }
}
