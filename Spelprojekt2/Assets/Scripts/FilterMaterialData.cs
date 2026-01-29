using UnityEngine;
using System;

[System.Serializable]
public struct FilterMaterial
{
    public Material m_deactivatedMaterial;
    public Material m_activatedMaterial;
}

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/FilterMaterialData", order = 1)]
public class FilterMaterialData : ScriptableObject
{
    public FilterMaterial[] m_materials;
}
