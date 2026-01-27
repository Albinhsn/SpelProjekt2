using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;


[CustomEditor(typeof(FilterManagerData))]
public class FilterManagerEditor : Editor
{

    public static void BeginCenterElement()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
    }
    public static void EndCenterElement()
    {
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    public override void OnInspectorGUI()
    {
        FilterManagerData obj = (FilterManagerData)target;

        int filterKindCount = (int)FilterKind.COUNT;

        if(obj.m_actions == null)
        {
            obj.m_actions = new FilterAction[filterKindCount];
        }

        SerializedObject serializedObj = new SerializedObject(obj);
        SerializedProperty actions      = serializedObj.FindProperty("m_actions");
        for(int i = 0; i < filterKindCount; i++)
        {
            FilterKind kind = (FilterKind)i;

            BeginCenterElement();
            GUILayout.Label(kind.ToString());
            EndCenterElement();

            SerializedProperty element = actions.GetArrayElementAtIndex(i);
            EditorGUILayout.PropertyField(element.FindPropertyRelative("action"), false);

            obj.m_actions[i].kind   = kind;

        }
        serializedObj.ApplyModifiedProperties();
        EditorUtility.SetDirty(obj);
    }
}