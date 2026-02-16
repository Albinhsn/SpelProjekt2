using Interaction;
using UnityEngine;
using UnityEngine.Rendering;

public enum FilterColor
{
    Red,
    Green,
    Blue,
    Yellow,
    COUNT
}

public enum FilterKind
{
    Primary,
    Secondary,
    COUNT,
    None,
}

[RequireComponent(typeof(MeshRenderer), typeof(Collider))]
public class FilterObject : MonoBehaviour
{
    public FilterKind m_kind;

    public bool Activated => m_activated;
    private bool m_activated;
    private MeshRenderer m_renderer;
    private Collider m_collider;
    private Rigidbody m_rb;
    private Interactable m_interactableComponent;

    private Material m_deactivatedMaterial;
    private Material m_activatedMaterial;
    // private float m_distanceToPlayer;
    // private FilterManager m_fm;
    // private GameObject m_player;
    


    public static int TagIsFilter(string tag)
    {
        for(int i = 0; i < (int)FilterKind.COUNT; i++)
        {
            if(tag == ((FilterKind)i).ToString())
            {
                return i;
            }
        }
        return -1;
    }

    void Awake()
    {
        if(m_kind == FilterKind.None)
        {
            Debug.LogError("FilterKind should never be set to None, needs to be primary or secondary");
        }

        m_renderer = GetComponent<MeshRenderer>();
        m_collider = GetComponent<Collider>();
        m_rb       = GetComponent<Rigidbody>();
        m_interactableComponent = GetComponent<Interactable>();

        this.gameObject.tag = this.m_kind.ToString();

        // m_player = GameObject.FindGameObjectWithTag("Player");

        FilterManager fm = FindFirstObjectByType<FilterManager>();

        if(fm != null && fm.m_filterColorData != null && fm.m_filterMaterialData != null)
        {
            ChangeMaterialColor(fm.m_filterColorData, fm.m_filterMaterialData);   
        }
    }

    // void Update()
    // {
    //     m_distanceToPlayer = Vector3.Distance(m_player.transform.position, transform.position);

    //     if(m_distanceToPlayer > m_fm.m_filterEffectDistance)
    //     {
    //         if(Activated)
    //         {
    //             Deactivate();
    //         }
    //     }
    //     else
    //     {
    //         if(m_kind == FilterManager.m_activeFilter && !Activated)
    //         {
    //             Activate();
    //         }
    //     }
    // }

    public void ChangeMaterialColor(FilterColorData filterColorData, FilterMaterialData material_data)
    {

        FilterColor color = filterColorData.m_Colors[(int)m_kind];
        
        this.m_deactivatedMaterial = material_data.m_materials[(int)color].m_deactivatedMaterial;
        this.m_activatedMaterial   = material_data.m_materials[(int)color].m_activatedMaterial;

        if(m_renderer != null)
        {
            m_renderer.material = m_activated ? m_activatedMaterial : m_deactivatedMaterial;
        }

    }

    public void Activate()
    {
        m_renderer.material = m_activatedMaterial;
        m_activated = true;

        if(m_collider != null)
        {
            // NOTE(ah): We can't turn off collision entirely because we need to
            // still know whether we're colliding with the object to avoid turning
            // off a filter while inside an object of that type (say player standing
            // inside it).
            m_collider.isTrigger = true;
        }

        if(m_rb != null)
        {
            m_rb.isKinematic = true;
        }
        
        m_interactableComponent?.SetIsInteractable(true);
    }

    public void Deactivate()
    {
        m_activated = false;
        m_renderer.material = m_deactivatedMaterial;
        if(m_collider != null)
        {
            m_collider.isTrigger = false;
        }

        if(m_rb != null)
        {
            m_rb.isKinematic = false;
        }
        
        m_interactableComponent?.SetIsInteractable(false);
    }
}
