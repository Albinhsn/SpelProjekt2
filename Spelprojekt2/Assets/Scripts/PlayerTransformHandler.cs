using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;
using static LinAlg.LinAlg;

public class PlayerTransformHandler : MonoBehaviour 
{ 
    [SerializeField,Tooltip("The speed at which the player rotates to face the direction of movement")] float m_transformRotationSpeed = .4f; 
    [SerializeField] private float m_transmarencyStartDistance = 2f;
    [SerializeField] private float m_fullTransparencyDistance = 1f;
    [SerializeField] private SkinnedMeshRenderer m_meshRenderer;
    private PlayerCamera m_camera; 
    private Rigidbody m_parentRb; 
    private Color m_originalColor;
    private MovementController m_movementController;
    
    void Awake() 
    { 
        m_originalColor = m_meshRenderer != null ? m_meshRenderer.material.color : new Color(1,1,1,1);
        m_camera = FindFirstObjectByType<PlayerCamera>(); 
        m_parentRb = transform.parent.GetComponent<Rigidbody>(); 
        m_movementController = GetComponentInParent<MovementController>();
    
    } 
    void FixedUpdate() 
    { 
        Vector3 forward = Rejection(InputManager.GetForwardAimDir(), new Vector3(0, 1, 0)); 
        Vector3 right = Rejection(InputManager.GetRightAimDir(), new Vector3(0, 1, 0)); 
        Vector2 input = InputManager.ReadMovementValue(); 
        Vector3 dir = forward * input.y + right * input.x; 
        
        if(!m_movementController.m_isJumping)
        {
            if(m_parentRb.linearVelocity.magnitude > 0.1f && input != Vector2.zero) 
            { 
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir.normalized, Vector3.up), m_transformRotationSpeed); 
            } 
            else if(forward != Vector3.zero)
            { 
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(forward.normalized, Vector3.up), m_transformRotationSpeed); 
            } 
        }
        else 
        {
            Vector3 xzVelocity = new Vector3(m_parentRb.linearVelocity.x,0,m_parentRb.linearVelocity.z);
            if(xzVelocity.magnitude > .01f)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(xzVelocity.normalized, Vector3.up), m_transformRotationSpeed); 
            }
        }
        
        
        HandleTransparancy();
    } 

    void HandleTransparancy()
    {
        float currentDistance = (transform.position - m_camera.transform.position).magnitude;
        float alphaResult = 1f - Mathf.InverseLerp(m_transmarencyStartDistance , m_fullTransparencyDistance, currentDistance);

        if(currentDistance < m_fullTransparencyDistance)
        {
            alphaResult = 0;
        }

        if(m_meshRenderer != null)
        {
            m_meshRenderer.material.color = new Color(m_originalColor.r, m_originalColor.g, m_originalColor.b, alphaResult);
        }
    }
}
