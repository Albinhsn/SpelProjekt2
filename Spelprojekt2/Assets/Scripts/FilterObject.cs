using UnityEngine;
using UnityEngine.Rendering;

public enum FilterKind
{
    Red,
    Blue,
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

    private Material m_deactivatedMaterial;
    private Material m_activatedMaterial;


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

        FilterMaterialData material_data = Resources.Load("StandardFilterMaterialData") as FilterMaterialData;
        this.m_deactivatedMaterial = material_data.m_materials[(int)m_kind].m_deactivatedMaterial;
        this.m_activatedMaterial   = material_data.m_materials[(int)m_kind].m_activatedMaterial;


        m_renderer = GetComponent<MeshRenderer>();
        m_collider = GetComponent<Collider>();
        m_rb       = GetComponent<Rigidbody>();

        m_renderer.material = m_deactivatedMaterial;


        this.gameObject.tag = this.m_kind.ToString();
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
    }
}
