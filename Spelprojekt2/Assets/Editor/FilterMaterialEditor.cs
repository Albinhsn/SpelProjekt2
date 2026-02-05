using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;


[CustomEditor(typeof(FilterMaterialData))]
public class FilterMaterialEditor : Editor
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
        FilterMaterialData obj = (FilterMaterialData)target;

        int filterColorCount = (int)FilterColor.COUNT;

        if(obj.m_materials == null || obj.m_materials.Length != filterColorCount)
        {
            obj.m_materials = new FilterMaterial[filterColorCount];
        }

        SerializedObject serializedObj = new SerializedObject(obj);
        SerializedProperty materials   = serializedObj.FindProperty("m_materials");
        for(int i = 0; i < filterColorCount; i++)
        {
            FilterColor color = (FilterColor)i;

            BeginCenterElement();
            GUILayout.Label(color.ToString());
            EndCenterElement();

            SerializedProperty element = materials.GetArrayElementAtIndex(i);
            EditorGUILayout.PropertyField(element.FindPropertyRelative("m_deactivatedMaterial"), false);
            EditorGUILayout.PropertyField(element.FindPropertyRelative("m_activatedMaterial"), false);

        }
        serializedObj.ApplyModifiedProperties();
        EditorUtility.SetDirty(obj);
    }
}
