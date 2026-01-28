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

    [SerializeField]
    private Material m_deactivatedMaterial;
    [SerializeField]
    private Material m_activatedMaterial;

    public bool Activated => m_activated;
    private bool m_activated;
    private MeshRenderer m_renderer;
    private Collider m_collider;
    private Rigidbody m_rb;

    void Awake()
    {
        m_renderer = GetComponent<MeshRenderer>();
        m_collider = GetComponent<Collider>();
        m_rb       = GetComponent<Rigidbody>();

        m_renderer.material = m_deactivatedMaterial;
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
