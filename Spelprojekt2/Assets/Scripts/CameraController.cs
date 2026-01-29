using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    private InputAction m_lookAction;
    float m_mouseSensitivity = 0.5f;
    [SerializeField]Transform parent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        m_lookAction = GetComponent<PlayerInput>().actions["Look"];
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 input = m_lookAction.ReadValue<Vector2>();
        transform.RotateAround(parent.position,parent.transform.up, input.x * m_mouseSensitivity);
    }
}
