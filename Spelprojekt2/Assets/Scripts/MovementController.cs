using UnityEngine;
using UnityEngine.InputSystem;
using static LinAlg.LinAlg;

[RequireComponent(typeof(Rigidbody))]
public class MovementController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float m_speed;

    [SerializeField] 
    private float m_jumpSpeed;

    [SerializeField]
    [Range(0.1f, 10.0f)]
    
    private float m_mouseSensitivity;
    private Rigidbody m_rb;
    private bool m_isJumping;
    private InputAction m_moveAction, m_lookAction, m_jumpAction;
    [SerializeField] Transform m_child;

    void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
        m_moveAction = GetComponent<PlayerInput>().actions["Move"];
        // m_lookAction = GetComponent<PlayerInput>().actions["Look"];
        m_jumpAction = GetComponent<PlayerInput>().actions["Jump"];
    }

    private void OnTriggerEnter(Collider other)
    {
        m_isJumping = false;
    }
    
    void Update()
    {
        // Vector2 input   = m_lookAction.ReadValue<Vector2>();
        // transform.Rotate(0, input.x * m_mouseSensitivity, 0);
        if(m_jumpAction.WasPressedThisFrame() && !m_isJumping)
        {
            m_rb.linearVelocity += new Vector3(0, m_jumpSpeed, 0);
            m_isJumping = true;
        }
    }

    void FixedUpdate()
    {
        // HACK(ah): Really poor movement, pls swap
        Vector3 forward = Rejection(m_child.forward, new Vector3(0, 1, 0));
        Vector3 right   = Rejection(transform.right, new Vector3(0, 1, 0));
        Vector2 input   = m_moveAction.ReadValue<Vector2>();
        Vector3 dir     = forward * input.y + right * input.x;
        if(dir.sqrMagnitude > 0)
        {
            dir = dir.normalized;
        }
        transform.position = transform.position + m_speed * dir * Time.fixedDeltaTime;
    }
}
