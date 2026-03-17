using UnityEngine;
using UnityEngine.Events;

public class UIButton : MonoBehaviour
{

    private float m_hotT;
    [SerializeField]
    private Camera m_camera;

    [SerializeField]
    private Material m_material;

    [SerializeField]
    private Material m_hotMaterial;

    [SerializeField]
    private MeshRenderer m_rend;

    [SerializeField]
    private Collider m_collider;

    [SerializeField]
    private UnityEvent m_onClickEvent;

    void Awake()
    {

    }

    void OnEnable()
    {
        Awake();
    }


    void Update()
    {

        Ray ray = m_camera.ScreenPointToRay(InputManager.ReadPointerPosition());
        bool hot = false;
        if(Physics.Raycast(ray, out var hit, Mathf.Infinity))
        {
            if(hit.collider == this.m_collider)
            {
                hot = true;
                m_rend.material = m_hotMaterial;
            }
        }

        if(!hot)
        {
            m_rend.material = m_material;
        }

        if(hot && InputManager.UIPointerSelect())
        {
            m_onClickEvent.Invoke();
        }
    }
}
