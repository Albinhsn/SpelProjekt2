using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FilterColorData))]
public class FilterColorDataEditor : Editor
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
        FilterColorData obj = (FilterColorData)target;

        int filterKindCount = (int)FilterKind.COUNT;

        if(obj.m_Colors == null || obj.m_Colors.Length != filterKindCount)
        {
            obj.m_Colors = new FilterColor[filterKindCount];
        }

        SerializedObject serializedObj = new SerializedObject(obj);
        SerializedProperty colors   = serializedObj.FindProperty("m_Colors");
        for(int i = 0; i < filterKindCount; i++)
        {
            SerializedProperty element = colors.GetArrayElementAtIndex(i);
            EditorGUILayout.PropertyField(element, new GUIContent(((FilterKind)i).ToString()), false);
        }
        serializedObj.ApplyModifiedProperties();
        EditorUtility.SetDirty(obj);
    }
}
