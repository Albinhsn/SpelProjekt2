using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    [Range(0.1f, 10.0f)]
    
    private float m_mouseSensitivity = 0.5f;
    [SerializeField] private float m_additionalYAngle = 45f;
    private InputAction m_lookAction;
    private float m_pitch = 0f;
    private Transform m_parent;
    private InputSystem_Actions m_inputActions;

    void Awake()
    {
        m_parent = transform.parent;
    }
    void Update()
    {
        //Read input value
        Vector2 input = m_lookAction.ReadValue<Vector2>();

        //Rotate camera around X axis
        transform.RotateAround(
            m_parent.position,
            m_parent.transform.up,
            input.x * m_mouseSensitivity);

        //Calculate and clamp pitch
        float pitchDelta = input.y * m_mouseSensitivity/2;
        float newPitch = m_pitch + pitchDelta;
        newPitch = Mathf.Clamp(newPitch, -m_additionalYAngle, m_additionalYAngle);

        //Remove pitch delta that would exceed clamped values
        float clampedDelta = newPitch - m_pitch;
        m_pitch = newPitch;

        //Apply clamped pitch rotation to Y axis
        transform.RotateAround(
            m_parent.position,
            transform.right,
            -clampedDelta);
    }

    //Enable and disable input actions
    void OnEnable()
    {
        m_inputActions = new();
        m_lookAction = m_inputActions.Player.Look;
        m_inputActions.Enable();
    }
    void OnDisable()
    {
        m_inputActions.Disable();
    }
}
