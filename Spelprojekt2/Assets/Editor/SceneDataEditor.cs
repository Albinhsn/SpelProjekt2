using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;


[CustomEditor(typeof(SceneData))]
public class SceneDataEditor : Editor
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
        SceneData obj = (SceneData)target;


        SerializedObject serializedObj = new SerializedObject(obj);
        serializedObj.Update();

        SerializedProperty subscenes = serializedObj.FindProperty("m_subscenes");
        if(subscenes != null)
        {
            EditorGUILayout.PropertyField(subscenes, includeChildren : true);
        }

        serializedObj.ApplyModifiedProperties();
    }
}
