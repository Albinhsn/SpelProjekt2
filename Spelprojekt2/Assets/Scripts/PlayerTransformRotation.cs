using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using static LinAlg.LinAlg;

public class PlayerTransformRotation : MonoBehaviour 
{ 
    [SerializeField,Tooltip("The speed at which the player rotates to face the direction of movement")] float m_transformRotationSpeed = .4f; 
    private PlayerCamera m_camera; 
    private Rigidbody m_parentRb; 
    void Awake() 
    { 
        m_camera = FindFirstObjectByType<PlayerCamera>(); 
        m_parentRb = transform.parent.GetComponent<Rigidbody>(); 
    
    } 
    void FixedUpdate() 
    { 
        Vector3 forward = Rejection(m_camera.m_playerForward, new Vector3(0, 1, 0)); 
        Vector3 right = Rejection(m_camera.m_playerRight, new Vector3(0, 1, 0)); 
        Vector2 input = InputManager.ReadMovementValue(); 
        Vector3 dir = forward * input.y + right * input.x; 
        if(m_parentRb.linearVelocity.magnitude > 0.1f && input != Vector2.zero) 
        { 
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir.normalized, Vector3.up), m_transformRotationSpeed); 
        } 
        else 
        { 
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(forward.normalized, Vector3.up), m_transformRotationSpeed); 
        } 
    } 
}