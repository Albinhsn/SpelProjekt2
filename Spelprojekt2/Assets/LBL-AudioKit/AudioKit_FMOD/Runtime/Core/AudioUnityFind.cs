using UnityEngine;

// AudioKit anteckning
// Unity 2023+ har nya "FindAny/FindFirstObjectByType"-API:er.
// Denna helper håller koden bakåtkompatibel och tar bort CS0618-varningar.

namespace AudioKit.FMOD
{
    public static class AudioUnityFind
    {
        public static T FindAny<T>(bool includeInactive = false) where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindAnyObjectByType<T>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude);
#elif UNITY_2020_1_OR_NEWER
            return Object.FindObjectOfType<T>(includeInactive);
#else
            return Object.FindObjectOfType(typeof(T)) as T;
#endif
        }

        public static T FindFirst<T>(bool includeInactive = false) where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType<T>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude);
#elif UNITY_2020_1_OR_NEWER
            return Object.FindObjectOfType<T>(includeInactive);
#else
            return Object.FindObjectOfType(typeof(T)) as T;
#endif
        }
    }
}
