using UnityEngine;
using UnityEngine.Rendering;

public enum FilterKind
{
    Red,
    Blue,
    COUNT,
}

[RequireComponent(typeof(MeshRenderer), typeof(Collider))]
public class FilterObject : MonoBehaviour
{
    public FilterKind m_kind;

    [SerializeField]
    private Material m_deactivatedMaterial;
    [SerializeField]
    private Material m_activatedMaterial;

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

        if(m_collider != null)
        {
            // NOTE(ah): This should probably just exclude a layer instead?
            m_collider.enabled = false;
        }

        if(m_rb != null)
        {
            m_rb.isKinematic = true;
        }
    }

    public void Deactivate()
    {
        m_renderer.material = m_deactivatedMaterial;
        if(m_collider != null)
        {
            m_collider.enabled = true;
        }

        if(m_rb != null)
        {
            m_rb.isKinematic = false;
        }
    }
}
