using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/FilterManagerData", order = 1)]
public class FilterManagerData : ScriptableObject
{
    public FilterAction[] m_actions;
}
