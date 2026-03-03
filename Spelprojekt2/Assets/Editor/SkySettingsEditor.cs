using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

[CustomEditor(typeof(SkySettings))]
public class SkySettingsEditor : Editor
{
    static void BeginCenterElement()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
    }
    static void EndCenterElement()
    {
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    public override void OnInspectorGUI()
    {
        SkySettings obj = (SkySettings)target;


        SerializedObject serializedObj = new SerializedObject(obj);
        serializedObj.Update();

        SerializedProperty data = serializedObj.FindProperty("m_data");
        if(data != null)
        {
            EditorGUILayout.PropertyField(data, includeChildren : true);
        }

        serializedObj.ApplyModifiedProperties();
        EditorUtility.SetDirty(obj);


        if(GUILayout.Button("Apply Settings"))
        {
            SkySettings.Apply(obj);
        }
    }

}
