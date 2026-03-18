using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ChangeMaterialToFilteredKind : MonoBehaviour
{
    [SerializeField] private FilterKind m_kind;
    [SerializeField] private MeshRenderer m_rend;

    void Start()
    {
        if(m_rend == null)
        {
            m_rend = GetComponent<MeshRenderer>();
        }

        if(m_rend != null)
        {
            m_rend.material = new Material(m_rend.material);
        }
        else
        {
            Debug.LogWarning("You need to either assign a renderer or have it active on start in ChangeMaterialToFilteredKind");
        }
    }

    public void ChangeMaterialColor(FilterColorData filterColorData, FilterMaterialData material_data)
    {

        if(m_rend == null)
        {
            Start();
        }

        FilterColor color = filterColorData.m_Colors[(int)m_kind];
        
        var m_deactivatedMaterial = material_data.m_materials[(int)color].m_deactivatedMaterial;

        if(m_rend != null)
        {
            m_rend.material.color = m_deactivatedMaterial.color;
        }

    }

}
