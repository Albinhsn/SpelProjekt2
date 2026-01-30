#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

// AudioKit anteckning
// Editor meny för att skapa assets
// Snabb setup utan klickkaos
// Skapar config och cues


namespace AudioKit.FMOD
{
    public static class AudioTools
    {
        private const string ResourcesAudio = "Assets/Resources/Audio";

        [MenuItem("AudioKit/Ljud/Skapa AudioConfig i Resources")]
        public static void CreateConfig()
        {
            EnsureFolder(ResourcesAudio);

            var path = Path.Combine(ResourcesAudio, "AudioConfig.asset").Replace("\\", "/");
            var existing = AssetDatabase.LoadAssetAtPath<AudioConfigSO>(path);
            if (existing != null)
            {
                Selection.activeObject = existing;
                return;
            }

            var asset = ScriptableObject.CreateInstance<AudioConfigSO>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
        }

        [MenuItem("AudioKit/Ljud/Skapa FootstepLibrary i Resources")]
        public static void CreateFootsteps()
        {
            EnsureFolder(ResourcesAudio);

            var path = Path.Combine(ResourcesAudio, "FootstepLibrary.asset").Replace("\\", "/");
            var existing = AssetDatabase.LoadAssetAtPath<FootstepLibrarySO>(path);
            if (existing != null)
            {
                Selection.activeObject = existing;
                return;
            }

            var asset = ScriptableObject.CreateInstance<FootstepLibrarySO>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
        }

        
        [MenuItem("AudioKit/Ljud/Skapa EventRegistry i Resources")]
        public static void CreateEventRegistry()
        {
            EnsureFolder(ResourcesAudio);

            var path = Path.Combine(ResourcesAudio, "EventRegistry.asset").Replace("\\", "/");
            var existing = AssetDatabase.LoadAssetAtPath<AudioEventRegistrySO>(path);
            if (existing != null)
            {
                Selection.activeObject = existing;
                return;
            }

            var asset = ScriptableObject.CreateInstance<AudioEventRegistrySO>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
        }

        [MenuItem("AudioKit/Ljud/Skapa AudioCue")]
        public static void CreateCue()
        {
            var folder = GetSelectedFolderPath();
            if (string.IsNullOrEmpty(folder))
                folder = "Assets";

            var asset = ScriptableObject.CreateInstance<AudioCueSO>();
            var path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folder, "AC_New.asset").Replace("\\", "/"));
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            var parts = path.Split('/');
            var current = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static string GetSelectedFolderPath()
        {
            var obj = Selection.activeObject;
            if (obj == null) return null;

            var path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path)) return null;

            if (AssetDatabase.IsValidFolder(path)) return path;

            return Path.GetDirectoryName(path).Replace("\\", "/");
        }
    }
}
#endif
