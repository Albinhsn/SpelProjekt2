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
    private PlayerCamera m_camera; 
    private Rigidbody m_parentRb; 
    private MeshRenderer m_meshRenderer;
    private Color m_originalColor;
    
    void Awake() 
    { 
        m_originalColor = GetComponent<MeshRenderer>().material.color;
        m_meshRenderer = GetComponent<MeshRenderer>();
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

        m_meshRenderer.material.color = new Color(m_originalColor.r, m_originalColor.g, m_originalColor.b, alphaResult);
        // }
        //  else
        // {
        //     m_meshRenderer.material.color = new Color(m_originalColor.r, m_originalColor.g, m_originalColor.b, m_originalColor.a);
        // }
    }
}