using System;
using UnityEngine;

namespace SP2.Audio
{
    [Serializable]
    public struct FootstepSet
    {
        public SurfaceType surface;

        [Header("== Step (walk/run) ==")]
        public AudioCueSO[] walkSteps;
        public AudioCueSO[] runSteps;

        [Header("== Jump / land (valfritt) ==")]
        public AudioCueSO[] jump;
        public AudioCueSO[] land;
    }

    // Placera asset i: Assets/Resources/Audio/FootstepLibrary.asset
    [CreateAssetMenu(menuName = "SP2/Audio/Footsteps/Footstep Library", fileName = "FootstepLibrary")]
    public sealed class FootstepLibrarySO : ScriptableObject
    {
        [Tooltip("Default används om en yta inte har en egen entry.")]
        public FootstepSet[] sets = Array.Empty<FootstepSet>();

        [Header("== Random seed (valfritt) ==")]
        public int randomSeed = 0;

        [NonSerialized] private bool _seeded;

        public AudioCueSO GetStepCue(SurfaceType surface, bool isRunning)
        {
            SeedOnce();
            var set = FindSet(surface);
            var list = isRunning ? set.runSteps : set.walkSteps;
            return Pick(list);
        }

        public AudioCueSO GetJumpCue(SurfaceType surface)
        {
            SeedOnce();
            return Pick(FindSet(surface).jump);
        }

        public AudioCueSO GetLandCue(SurfaceType surface)
        {
            SeedOnce();
            return Pick(FindSet(surface).land);
        }

        private FootstepSet FindSet(SurfaceType surface)
        {
            // Först: exakt match
            for (int i = 0; i < sets.Length; i++)
            {
                if (sets[i].surface == surface)
                    return sets[i];
            }

            // Fallback: Default
            for (int i = 0; i < sets.Length; i++)
            {
                if (sets[i].surface == SurfaceType.Default)
                    return sets[i];
            }

            // Sista fallback: tom
            return default;
        }

        private static AudioCueSO Pick(AudioCueSO[] list)
        {
            if (list == null || list.Length == 0)
                return null;

            // Välj slumpmässigt (UnityEngine.Random)
            int idx = UnityEngine.Random.Range(0, list.Length);
            return list[idx];
        }

        private void SeedOnce()
        {
            if (_seeded) return;
            _seeded = true;

            if (randomSeed != 0)
                UnityEngine.Random.InitState(randomSeed);
        }
    }
}
