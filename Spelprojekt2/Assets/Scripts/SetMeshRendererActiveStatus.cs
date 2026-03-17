using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class SetMeshRendererActiveStatus : MonoBehaviour
{
    private MeshRenderer m_rend;

    void Awake()
    {
        m_rend = GetComponent<MeshRenderer>();
    }

    void OnEnable()
    {
        Awake();
    }

    public void Activate()
    {
        m_rend.enabled = true;
    }

    public void Deactivate()
    {
        m_rend.enabled = false;
    }
}
