using UnityEngine;

// AudioKit anteckning
// Små hjälpfunktioner för att hitta rätt activator (t.ex. Player)
// Funkar med riggar där tag sitter på root eller på rigidbody

namespace AudioKit.FMOD
{
    internal static class AudioActivatorUtil
    {
        public static bool HasTag(Collider other, string tag)
        {
            if (other == null) return false;

            if (other.CompareTag(tag)) return true;

            var rb = other.attachedRigidbody;
            if (rb != null && rb.CompareTag(tag)) return true;

            var root = other.transform.root;
            if (root != null && root.CompareTag(tag)) return true;

            return false;
        }

        public static int GetActivatorKey(Collider other, string tag)
        {
            if (other == null) return 0;

            // Prioritera rigidbody om den är märkt
            var rb = other.attachedRigidbody;
            if (rb != null && (string.IsNullOrEmpty(tag) || rb.CompareTag(tag)))
                return rb.GetInstanceID();

            var root = other.transform.root;
            if (root != null && (string.IsNullOrEmpty(tag) || root.CompareTag(tag)))
                return root.GetInstanceID();

            return other.GetInstanceID();
        }
    }
}
