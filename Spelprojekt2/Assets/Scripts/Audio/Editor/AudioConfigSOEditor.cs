#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace SP2.Audio.Editor
{
    [CustomEditor(typeof(AudioConfigSO))]
    public sealed class AudioConfigSOEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox(
                "Tips: Lägg detta i Resources/Audio/AudioConfig.asset (Tools-fönstret kan skapa den åt dig).\n" +
                "Här sätts: main music event, param-namn, VCA-paths och default volymer.",
                MessageType.Info);

            DrawDefaultInspector();

            var cfg = (AudioConfigSO)target;
            GUILayout.Space(8);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Snabb-check", EditorStyles.boldLabel);

                if (cfg.mainMusicEvent.IsNull)
                    EditorGUILayout.HelpBox("MainMusicEvent är tom. Musik startar inte.", MessageType.Warning);

                if (string.IsNullOrEmpty(cfg.vcaMusic) || string.IsNullOrEmpty(cfg.vcaSfx) || string.IsNullOrEmpty(cfg.vcaUi))
                    EditorGUILayout.HelpBox("Någon VCA-path är tom.", MessageType.Warning);

                if (GUILayout.Button("Ping AudioTools"))
                    EditorApplication.ExecuteMenuItem("SP2/Audio/Tools");
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
