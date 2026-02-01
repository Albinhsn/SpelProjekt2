#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AudioKit.FMOD
{
    /// <summary>
    /// Inspector-drawer för AudioParamRef.
    /// Stödjer tre lägen:
    ///  - Asset: välj AudioParameterSO
    ///  - Key: skriv en key som mappas via AudioParamLibrary
    ///  - Namn: skriv FMOD-parameterns namn direkt
    ///
    /// Viktigt: läget ska kunna bytas även om fälten är tomma.
    /// Därför sparas senaste läget per property i SessionState.
    /// </summary>
    [CustomPropertyDrawer(typeof(AudioParamRef))]
    public sealed class AudioParamRefDrawer : PropertyDrawer
    {
        private enum Mode { Asset = 0, Key = 1, Name = 2 }

        private static readonly string[] ModeLabels = { "Asset", "Key", "Namn" };

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // 2 rader: mode + value
            var line = EditorGUIUtility.singleLineHeight;
            var space = EditorGUIUtility.standardVerticalSpacing;
            return (line * 2f) + space;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var parameterProp = property.FindPropertyRelative("parameter");
            var keyProp       = property.FindPropertyRelative("key");
            var nameProp      = property.FindPropertyRelative("name");

            var line  = EditorGUIUtility.singleLineHeight;
            var space = EditorGUIUtility.standardVerticalSpacing;

            var rMode  = new Rect(position.x, position.y, position.width, line);
            var rValue = new Rect(position.x, position.y + line + space, position.width, line);

            // Läges-nyckel per objekt + propertypath (per-session)
            var target = property.serializedObject.targetObject;
            var prefKey = $"AudioKit.AudioParamRef.Mode.{target.GetInstanceID()}.{property.propertyPath}";

            Mode mode = DetectMode(parameterProp, keyProp, nameProp, prefKey);

            // Om asset råkar vara satt, tvinga Asset-läge och rensa textfälten
            if (parameterProp.objectReferenceValue != null && mode != Mode.Asset)
            {
                mode = Mode.Asset;
                SessionState.SetInt(prefKey, (int)mode);
                keyProp.stringValue = string.Empty;
                nameProp.stringValue = string.Empty;
                property.serializedObject.ApplyModifiedProperties();
            }

            // Mode popup
            int cur = (int)mode;
            int next = EditorGUI.Popup(rMode, "Parameter", cur, ModeLabels);

            if (next != cur)
            {
                mode = (Mode)next;
                SessionState.SetInt(prefKey, next);

                // En källa i taget: när man väljer Key/Namn så rensas Asset.
                switch (mode)
                {
                    case Mode.Asset:
                        keyProp.stringValue = string.Empty;
                        nameProp.stringValue = string.Empty;
                        break;

                    case Mode.Key:
                        parameterProp.objectReferenceValue = null;
                        nameProp.stringValue = string.Empty;
                        break;

                    case Mode.Name:
                        parameterProp.objectReferenceValue = null;
                        keyProp.stringValue = string.Empty;
                        break;
                }

                property.serializedObject.ApplyModifiedProperties();
            }

            // Value field
            switch (mode)
            {
                case Mode.Asset:
                    EditorGUI.PropertyField(rValue, parameterProp, new GUIContent("Parameter"));
                    break;

                case Mode.Key:
                    EditorGUI.PropertyField(rValue, keyProp, new GUIContent("Key"));
                    break;

                case Mode.Name:
                    EditorGUI.PropertyField(rValue, nameProp, new GUIContent("FMOD-namn"));
                    break;
            }

            EditorGUI.EndProperty();
        }

        private static Mode DetectMode(SerializedProperty parameterProp, SerializedProperty keyProp, SerializedProperty nameProp, string prefKey)
        {
            if (parameterProp != null && parameterProp.objectReferenceValue != null)
                return Mode.Asset;

            var key = keyProp != null ? keyProp.stringValue : string.Empty;
            if (!string.IsNullOrWhiteSpace(key))
                return Mode.Key;

            var nm = nameProp != null ? nameProp.stringValue : string.Empty;
            if (!string.IsNullOrWhiteSpace(nm))
                return Mode.Name;

            // Om allt är tomt, använd senaste val (default Asset)
            int stored = SessionState.GetInt(prefKey, (int)Mode.Asset);
            if (stored < 0 || stored > 2) stored = (int)Mode.Asset;
            return (Mode)stored;
        }
    }
}
#endif
