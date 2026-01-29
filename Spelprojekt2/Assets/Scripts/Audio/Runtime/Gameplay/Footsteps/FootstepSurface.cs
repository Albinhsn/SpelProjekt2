using UnityEngine;

namespace SP2.Audio
{
    // Lägg på samma GameObject som Collider (eller på en parent och kryssa i "Affect Children").
    [DisallowMultipleComponent]
    public sealed class FootstepSurface : MonoBehaviour
    {
        [Header("== Surface ==")]
        public SurfaceType surfaceType = SurfaceType.Default;

        [Tooltip("Om true: den här ytan gäller även för colliders i barn-objekt.")]
        public bool affectChildren = true;
    }
}
