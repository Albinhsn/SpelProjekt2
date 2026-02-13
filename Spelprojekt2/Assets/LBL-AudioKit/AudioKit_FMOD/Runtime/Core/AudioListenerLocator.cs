using UnityEngine;
using FMODUnity;

// AudioKit anteckning
// Hjälp för att hitta en rimlig listener-transform.
// Prioritet: StudioListener -> Camera.main -> valfri Camera.

namespace AudioKit.FMOD
{
    public static class AudioListenerLocator
    {
        private static Transform cached;
        private static int cachedFrame = -1;

        public static Transform GetListenerTransform()
        {
            // Cache per frame (räcker, och undviker Find i Update-loops)
            if (cachedFrame == Time.frameCount && cached != null)
                return cached;

            cachedFrame = Time.frameCount;

            var studioListener = AudioUnityFind.FindAny<StudioListener>(true);
            if (studioListener != null)
            {
                cached = studioListener.transform;
                return cached;
            }

            var cam = Camera.main;
            if (cam == null) cam = AudioUnityFind.FindAny<Camera>(true);

            cached = cam != null ? cam.transform : null;
            return cached;
        }

        public static void Invalidate()
        {
            cached = null;
            cachedFrame = -1;
        }
    }
}
