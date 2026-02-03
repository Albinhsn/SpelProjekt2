using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using static LinAlg.LinAlg;

[RequireComponent(typeof(Rigidbody))]
public class MovementController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float m_speed;

    [SerializeField] 
    private float m_jumpSpeed;
    private Rigidbody m_rb;
    private bool m_isJumping;
    private InputAction m_moveAction, m_jumpAction;
    private InputSystem_Actions m_inputActions;
    private Transform m_cameraTransform;

    void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
        m_cameraTransform = Camera.main.transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        m_isJumping = false;
    }
    
    void Update()
    {
        if(m_jumpAction.WasPressedThisFrame() && !m_isJumping)
        {
            m_rb.linearVelocity += new Vector3(0, m_jumpSpeed, 0);
            m_isJumping = true;
        }
    }

    void FixedUpdate()
    {
        // Calculate movement direction
        Vector3 forward = Rejection(m_cameraTransform.forward, new Vector3(0, 1, 0));
        Vector3 right   = Rejection(m_cameraTransform.right, new Vector3(0, 1, 0));
        Vector2 input   = m_moveAction.ReadValue<Vector2>();
        Vector3 dir     = forward * input.y + right * input.x;

        // Normalize direction
        if(dir.sqrMagnitude > 0)
        {
            dir = dir.normalized;
        }

        // Apply movement
        m_rb.linearVelocity = new Vector3(
            dir.x * m_speed,
            m_rb.linearVelocity.y,
            dir.z * m_speed
        );
    }
    
    //Enable and disable input actions
    void OnEnable()
    {
        m_inputActions = new();
        m_moveAction = m_inputActions.Player.Move;
        m_jumpAction = m_inputActions.Player.Jump;
        m_inputActions.Enable();
    }
    void OnDisable()
    {
        m_inputActions.Disable();
    }
}
