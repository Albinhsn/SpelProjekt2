#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace SP2.Audio.Editor
{
    [CustomEditor(typeof(AudioCueSO))]
    public sealed class AudioCueSOEditor : UnityEditor.Editor
    {
        private EventInstance _preview;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox(
                "AudioCue = återanvändbar länk till ett FMOD-event.\n" +
                "Tips: använd cues i prefabs/logic så ni slipper duplicera EventReferences överallt.",
                MessageType.Info);

            DrawDefaultInspector();

            var cue = (AudioCueSO)target;
            GUILayout.Space(8);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Preview (Play Mode)", EditorStyles.boldLabel);

                if (!EditorApplication.isPlaying)
                {
                    EditorGUILayout.HelpBox("Starta Play Mode för att kunna previewa ljud.", MessageType.None);
                }
                else
                {
                    using (new EditorGUI.DisabledScope(cue.eventReference.IsNull))
                    {
                        GUILayout.BeginHorizontal();

                        if (GUILayout.Button("Play"))
                        {
                            StopPreview();
                            _preview = RuntimeManager.CreateInstance(cue.eventReference);
                            _preview.start();
                        }

                        if (GUILayout.Button("Stop"))
                        {
                            StopPreview();
                        }

                        GUILayout.EndHorizontal();
                    }

                    if (cue.eventReference.IsNull)
                        EditorGUILayout.HelpBox("EventReference är tom.", MessageType.Warning);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnDisable()
        {
            StopPreview();
        }

        private void StopPreview()
        {
            if (!_preview.isValid()) return;
            _preview.stop(STOP_MODE.ALLOWFADEOUT);
            _preview.release();
            _preview.clearHandle();
        }
    }
}
#endif
