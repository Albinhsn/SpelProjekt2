using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] float m_additionalYAngle = 45f;
    private InputAction m_lookAction;
    float m_mouseSensitivity = 0.5f;
    float m_pitch = 0f;
    Transform m_parent;
    private InputSystem_Actions m_inputActions;
    void Awake()
    {
        // m_lookAction = GetComponent<PlayerInput>().actions["Look"];
        m_parent = transform.parent;
    }

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

    void Update()
    {
        Vector2 input = m_lookAction.ReadValue<Vector2>();

        transform.RotateAround(
            m_parent.position,
            m_parent.transform.up,
            input.x * m_mouseSensitivity);

        float pitchDelta = input.y * m_mouseSensitivity/2;
        float newPitch = m_pitch + pitchDelta;
        newPitch = Mathf.Clamp(newPitch, -m_additionalYAngle, m_additionalYAngle);

        float clampedDelta = newPitch - m_pitch;
        m_pitch = newPitch;

        transform.RotateAround(
            m_parent.position,
            transform.right,
            -clampedDelta);
    }
}
