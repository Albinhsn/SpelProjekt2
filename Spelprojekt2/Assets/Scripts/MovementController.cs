using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using static LinAlg.LinAlg;

[RequireComponent(typeof(Rigidbody))]
public class MovementController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Tooltip("The movement speed of the player")]
    private float m_speed;

    [SerializeField,Tooltip("The force applied in y axis when the player jumps")] 
    private float m_jumpForce = 5.0f;
    [SerializeField,Range(0.0f, 1.0f), Tooltip("The higher the value, the smoother the acceleration/deceleration(0 = instant, 1 = no movement)")]
    private float m_smoothAccelerationFactor = 0.1f;
    private Rigidbody m_rb;
    private bool m_isJumping;
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
        if(InputManager.Jumped() && !m_isJumping)
        {
            m_rb.linearVelocity += new Vector3(0, m_jumpForce, 0);
            m_isJumping = true;
        }
    }

    void FixedUpdate()
    {
        // Calculate movement direction
        Vector3 forward = Rejection(m_cameraTransform.forward, new Vector3(0, 1, 0));
        Vector3 right   = Rejection(m_cameraTransform.right, new Vector3(0, 1, 0));
        Vector2 input   = InputManager.ReadMovementValue();
        Vector3 dir     = forward * input.y + right * input.x;

        // Normalize direction
        if(dir.sqrMagnitude > 0)
        {
            dir = dir.normalized;
        }

        // Apply movement
        m_rb.linearVelocity = Vector3.Lerp(new Vector3(
            dir.x * m_speed,
            m_rb.linearVelocity.y,
            dir.z * m_speed
        ), m_rb.linearVelocity, m_smoothAccelerationFactor);
    }
}
