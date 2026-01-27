using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class MovementController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float m_speed;

    [SerializeField]
    private float m_jumpSpeed;

    [SerializeField]
    private InputActionReference m_movementAction;

    [SerializeField] 
    private InputActionReference m_lookAction;

    [SerializeField]
    private InputActionReference m_jumpAction;

    [SerializeField]
    [Range(0.1f, 10.0f)]
    private float m_mouseSensitivity;

    private Rigidbody m_rb;
    private bool m_isJumping;

    void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        m_isJumping = false;
    }
    
    void Update()
    {
        Vector2 input   = m_lookAction.action.ReadValue<Vector2>();
        this.transform.Rotate(0, input.x * m_mouseSensitivity, 0);
        if(m_jumpAction.action.WasPressedThisFrame() && !m_isJumping)
        {
            m_rb.linearVelocity += new Vector3(0, m_jumpSpeed, 0);
            m_isJumping = true;
        }
    }

    // TODO(ah): Does this exist elsewhere or hoist to some library?
    public static Vector3 Rejection(Vector3 a, Vector3 b)
    {
        return a - Vector3.Project(a, b);
    }

    public static Vector3 Hadamard(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    void FixedUpdate()
    {
        // HACK(ah): Really poor movement, pls swap
        Vector3 forward = Rejection(this.transform.forward, new Vector3(0, 1, 0));
        Vector3 right   = Rejection(this.transform.right, new Vector3(0, 1, 0));
        Vector2 input   = m_movementAction.action.ReadValue<Vector2>();
        Vector3 dir     = forward * input.y + right * input.x;
        if(dir.sqrMagnitude > 0)
        {
            dir = dir.normalized;
        }
        this.transform.position = this.transform.position + m_speed * dir * Time.fixedDeltaTime;
    }
}
