using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using static LinAlg.LinAlg;
public class PlayerTransformRotation : MonoBehaviour
{
    [SerializeField,Tooltip("The speed at which the player rotates to face the direction of movement")] float m_transformRotationSpeed = .4f;

    void FixedUpdate()
    {        
        Transform camera = FindFirstObjectByType<CinemachineCamera>().transform;

        Vector3 forward = Rejection(camera.forward, new Vector3(0, 1, 0));
        Vector3 right   = Rejection(camera.right, new Vector3(0, 1, 0));
        Vector2 input   = InputManager.ReadMovementValue();
        Vector3 dir     = forward * input.y + right * input.x;

        if(input == Vector2.zero) return;
        
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir.normalized, Vector3.up), m_transformRotationSpeed);
    }
}
