using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using static LinAlg.LinAlg;
public class PlayerTransformRotation : MonoBehaviour
{
    [SerializeField,Tooltip("The speed at which the player rotates to face the direction of movement")] float m_transformRotationSpeed = .4f;
    private Transform m_camera;
    void Awake()
    {
        m_camera = GetComponentInChildren<CinemachineCamera>().transform;
    }
    void FixedUpdate()
    {        

        Vector3 forward = Rejection(m_camera.forward, new Vector3(0, 1, 0));
        Vector3 right   = Rejection(m_camera.right, new Vector3(0, 1, 0));
        Vector2 input   = InputManager.ReadMovementValue();
        Vector3 dir     = forward * input.y + right * input.x;

        if(input == Vector2.zero) return;
        
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir.normalized, Vector3.up), m_transformRotationSpeed);
    }
}
