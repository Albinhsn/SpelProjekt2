using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ChangeMaterialToFilteredKind : MonoBehaviour
{
    [SerializeField] private FilterKind m_kind;

    void Start()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();

        renderer.material = new Material(renderer.material);
    }

    public void ChangeMaterialColor(FilterColorData filterColorData, FilterMaterialData material_data)
    {

        FilterColor color = filterColorData.m_Colors[(int)m_kind];
        
        var m_deactivatedMaterial = material_data.m_materials[(int)color].m_deactivatedMaterial;

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if(renderer != null)
        {
            renderer.material.color = m_deactivatedMaterial.color;
        }

    }

}
