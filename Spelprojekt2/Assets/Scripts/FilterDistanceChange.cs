using UnityEngine;

public class FilterDistanceChange : MonoBehaviour
{
    // FilterColorData m_filterColorData;
    // FilterMaterialData m_filterMaterialData;
    MeshRenderer m_renderer;
    Material m_primaryMaterial;
    Material m_secondaryMaterial;
    Material m_startMaterial;
    bool m_primaryActive;
    bool m_secondaryActive;
    void Awake()
    {
        m_renderer = GetComponent<MeshRenderer>();
        m_startMaterial = m_renderer.material;
        // m_filterMaterialData = Resources.Load("StandardFilterMaterialData") as FilterMaterialData;
        // m_filterColorData    = Resources.Load("ScriptableObjects/FilterColorData") as FilterColorData;
        FilterManager fm = FindFirstObjectByType<FilterManager>();
        fm.m_filterChanged.AddListener(HandleColorChange);
        if(fm != null && fm.m_filterColorData != null && fm.m_filterMaterialData != null)
        {
            transform.localScale = Vector3.one * fm.m_filterEffectDistance;
            ChangeMaterialColor(fm.m_filterColorData, fm.m_filterMaterialData);   
        }
    }

    void HandleColorChange(FilterKind kind, bool active)
    {
        if(active)
        {
            if(kind == FilterKind.Primary)
            {
                m_renderer.material = m_primaryMaterial;
                m_primaryActive = true;
                m_secondaryActive = false;
            }
            else if(kind == FilterKind.Secondary)
            {
                m_renderer.material = m_secondaryMaterial;
                m_secondaryActive = true;
                m_primaryActive = false;
            }
        }
        if(!active)
        {
            m_renderer.material = m_startMaterial;
            m_primaryActive = false;
            m_secondaryActive = false;
        }
    }

    public void ChangeMaterialColor(FilterColorData filterColorData, FilterMaterialData material_data)
    {

        FilterColor color = filterColorData.m_Colors[0];
        
        this.m_primaryMaterial = material_data.m_materials[(int)color].m_activatedMaterial;

        color = filterColorData.m_Colors[1];
        this.m_secondaryMaterial   = material_data.m_materials[(int)color].m_activatedMaterial;

        if(m_renderer != null)
        {
            if(m_primaryActive)
            {
                m_renderer.material = m_primaryMaterial;
            }
            else if(m_secondaryActive)
            {
                m_renderer.material = m_secondaryMaterial;
            }   
        }
    }


}
