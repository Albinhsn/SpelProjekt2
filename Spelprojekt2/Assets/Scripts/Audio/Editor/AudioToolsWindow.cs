#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace SP2.Audio.Editor
{
    public sealed class AudioToolsWindow : EditorWindow
    {
        private const string RES_AUDIO_PATH = "Assets/Resources/Audio";

        private const string CONFIG_ASSET_PATH = "Assets/Resources/Audio/AudioConfig.asset";
        private const string FOOTSTEPS_ASSET_PATH = "Assets/Resources/Audio/FootstepLibrary.asset";

        [MenuItem("SP2/Audio/Tools")]
        public static void Open()
        {
            var w = GetWindow<AudioToolsWindow>(false, "SP2 Audio Tools", true);
            w.minSize = new Vector2(420, 340);
        }

        private void OnGUI()
        {
            GUILayout.Label("SP2 Audio Tools", EditorStyles.boldLabel);
            GUILayout.Space(6);

            DrawConfig();
            GUILayout.Space(8);

            DrawFootsteps();
            GUILayout.Space(8);

            DrawQuickFolders();
            GUILayout.Space(8);

            DrawSanity();
            GUILayout.Space(8);

            DrawExtras();
        }

        private static void DrawConfig()
        {
            var cfg = AssetDatabase.LoadAssetAtPath<AudioConfigSO>(CONFIG_ASSET_PATH);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("AudioConfig", EditorStyles.boldLabel);

                if (cfg != null)
                {
                    EditorGUILayout.ObjectField("Config", cfg, typeof(AudioConfigSO), false);

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Select"))
                        Selection.activeObject = cfg;
                    if (GUILayout.Button("Ping"))
                        EditorGUIUtility.PingObject(cfg);
                    GUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.HelpBox("Hittar ingen AudioConfig i Resources. Skapa en med knappen nedan.", MessageType.Warning);

                    if (GUILayout.Button("Create AudioConfig (Resources/Audio)"))
                    {
                        EnsureFolder(RES_AUDIO_PATH);
                        var asset = CreateInstance<AudioConfigSO>();
                        AssetDatabase.CreateAsset(asset, CONFIG_ASSET_PATH);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        Selection.activeObject = asset;
                        EditorGUIUtility.PingObject(asset);
                    }
                }
            }
        }

        private static void DrawFootsteps()
        {
            var lib = AssetDatabase.LoadAssetAtPath<FootstepLibrarySO>(FOOTSTEPS_ASSET_PATH);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Footsteps (valfritt)", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Skapar en FootstepLibrary i Resources så FootstepController kan auto-ladda den utan inspector-setup.", MessageType.None);

                if (lib != null)
                {
                    EditorGUILayout.ObjectField("Library", lib, typeof(FootstepLibrarySO), false);

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Select"))
                        Selection.activeObject = lib;
                    if (GUILayout.Button("Ping"))
                        EditorGUIUtility.PingObject(lib);
                    GUILayout.EndHorizontal();
                }
                else
                {
                    if (GUILayout.Button("Create FootstepLibrary (Resources/Audio)"))
                    {
                        EnsureFolder(RES_AUDIO_PATH);

                        var asset = CreateInstance<FootstepLibrarySO>();
                        // Skapa en minimal default entry så assetet inte är tomt
                        asset.sets = new FootstepSet[]
                        {
                            new FootstepSet { surface = SurfaceType.Default }
                        };

                        AssetDatabase.CreateAsset(asset, FOOTSTEPS_ASSET_PATH);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        Selection.activeObject = asset;
                        EditorGUIUtility.PingObject(asset);
                    }
                }
            }
        }

        private static void DrawQuickFolders()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Snabbmappar", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Tips: skapa en mapp t.ex. Assets/Audio/Cues och gör AudioCue-assets där.", MessageType.Info);

                if (GUILayout.Button("Create Example Cue Folder (Assets/Audio/Cues)"))
                {
                    EnsureFolder("Assets/Audio");
                    EnsureFolder("Assets/Audio/Cues");
                    AssetDatabase.Refresh();
                }
            }
        }

        private static void DrawSanity()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Sanity", EditorStyles.boldLabel);

                if (GUILayout.Button("Clear FMOD caches (param/event)"))
                {
                    FmodSafe.ClearCaches();
                    Debug.Log("[SP2 Audio] FMOD caches cleared");
                }
            }
        }

        private static void DrawExtras()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Extras", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Valfritt: skapa ett debug overlay-objekt i aktuell scen (F9 för toggle).", MessageType.None);

                if (GUILayout.Button("Create AudioDebugOverlay (scene)"))
                {
                    if (Object.FindObjectOfType<SP2.Audio.AudioDebugOverlay>() == null)
                    {
                        var go = new GameObject("AudioDebugOverlay");
                        go.AddComponent<SP2.Audio.AudioDebugOverlay>();
                        Undo.RegisterCreatedObjectUndo(go, "Create AudioDebugOverlay");
                        Selection.activeObject = go;
                    }
                    else
                    {
                        Debug.Log("[SP2 Audio] AudioDebugOverlay finns redan i scenen");
                    }
                }
            }
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string leaf = Path.GetFileName(path);

            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);

            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
#endif
