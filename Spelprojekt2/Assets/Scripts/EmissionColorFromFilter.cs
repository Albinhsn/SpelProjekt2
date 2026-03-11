using UnityEngine;

public class EmissionColorFromFilter : MonoBehaviour
{
    [SerializeField]
    private Color m_defaultColor;

    [SerializeField]
    private float m_emissiveIntensity = 1.0f;
    private Material m_emissionMaterial;
    private FilterColorData m_filterColorData;
    private FilterMaterialData m_filterMaterialData;
    private SkinnedMeshRenderer m_rend;

    void Start()
    {
        SkinnedMeshRenderer renderer = GetComponent<SkinnedMeshRenderer>();
        m_rend = renderer;
        if(m_rend != null)
        {
            m_emissionMaterial = renderer.material;
            m_emissionMaterial.EnableKeyword("_EMISSION");
        }


        m_filterMaterialData = Resources.Load("StandardFilterMaterialData") as FilterMaterialData;
        m_filterColorData    = Resources.Load("ScriptableObjects/FilterColorData") as FilterColorData;
        FilterManager fm = FindFirstObjectByType<FilterManager>();
        fm.m_filterChanged.AddListener(UpdateFilter);
    }

    void OnDestroy()
    {
        FilterManager fm = FindFirstObjectByType<FilterManager>();
        if(fm != null)
        {
            fm.m_filterChanged.RemoveListener(UpdateFilter);
        }
    }

    void UpdateFilter(FilterKind filter, bool active)
    {
        if(m_rend != null)
        {
            if(!active)
            {
                m_emissionMaterial.SetColor("_EmissiveColor", m_defaultColor * m_emissiveIntensity);
            }
            else
            {

                FilterColor filter_color = m_filterColorData.m_Colors[(int)filter];
                Color color              = m_filterMaterialData.m_materials[(int)filter_color].m_deactivatedMaterial.color;

                m_emissionMaterial.SetColor("_EmissiveColor", color * m_emissiveIntensity);
                DynamicGI.UpdateEnvironment();
            }
            RendererExtensions.UpdateGIMaterials(m_rend);
        }
    }



}
