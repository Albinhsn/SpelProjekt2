using System;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Orbit")]
    [SerializeField] private float m_mouseSensitivity = 3f;
    [SerializeField] private float m_pitchClamp = 60f;
    [SerializeField] private float m_rotationSmoothTime = 0.05f;
    [Header("Third Person")]
    [SerializeField] private float m_thirdPersonCameraHeight = .5f;
    [SerializeField] private float m_decolliderSphereRadius = 1f;
    [Header("First Person")]
    [SerializeField] private float m_firstPersonCameraHeight = .75f;

    [Header("Zoom")]
    [SerializeField] private float m_minZoomDistance = 2f;
    [SerializeField] private float m_maxZoomDistance = 6f;
    [SerializeField] private float m_zoomSpeed = 5f;
    [SerializeField] private float m_zoomSmoothSpeed = 10f;

    [Header("Shoulder")]
    [SerializeField] private float m_shoulderOffset = 1.2f;
    [SerializeField] private float m_shoulderDisplacementStartDistance = 3f;
    [SerializeField] private float m_shoulderSmoothSpeed = 8f;

    private float m_targetYaw;
    private float m_targetPitch;
    private float m_currentYaw;
    private float m_currentPitch;
    private float m_yawVelocity;
    private float m_pitchVelocity;
    private float m_targetDistance;
    private float m_currentDistance;
    private float m_currentShoulderOffset;
    private bool m_isRightShoulder = true;
    private Transform m_parent;
    private float m_savedPitch;
    private float m_savedYaw;
    [HideInInspector] public Vector3 m_playerForward, m_playerRight;

    private void Start()
    {
        m_parent = transform.parent;
        Vector3 angles = transform.eulerAngles;
        m_targetYaw = m_currentYaw = angles.y;
        m_targetPitch = m_currentPitch = angles.x;
        m_targetDistance = m_currentDistance = m_maxZoomDistance;
    }
    void Update()
    {
        HandleShoulderSwitch();

        if(InputManager.CameraFreeLookToggleRealesed())
        {
            m_targetPitch = m_savedPitch;
            m_targetYaw = m_savedYaw;
        }

        if(!InputManager.CameraFreeLookToggleHeld())
        {
            m_savedPitch = m_targetPitch;
            m_savedYaw = m_targetYaw;
        }

        m_playerForward = Quaternion.Euler(m_savedPitch, m_savedYaw, 0) * Vector3.forward;
        m_playerRight = Quaternion.Euler(m_savedPitch, m_savedYaw, 0) * Vector3.right;

        InputManager.SetAimDirection(m_playerForward,m_playerRight);
    }

    private void FixedUpdate()
    {
        if(InputManager.CameraFirstPerson())
        {
            FirstPersonCamera();
        }
        else
        {
            ThirdPersonCamera();
        }
        
    }
    void ThirdPersonCamera()
    {
        HandleRotation();
        HandleZoom();
        
        UpdateCamera();
        Decollider();
    }
    void FirstPersonCamera()
    {
        HandleRotation();

        Vector3 fpPosition = m_parent.position + (Vector3.up * m_firstPersonCameraHeight);
        transform.position = fpPosition;

        Quaternion fpRotation = Quaternion.Euler(m_currentPitch, m_currentYaw, 0);
        transform.rotation = fpRotation;
    }
    void Decollider()
    {
        Vector3 camDir = (transform.position - (m_parent.position + (Vector3.up * m_thirdPersonCameraHeight))).normalized;
        RaycastHit hit;
        // Ray hit;

        if(Physics.Raycast(m_parent.position + (Vector3.up * m_thirdPersonCameraHeight), camDir, out hit, m_targetDistance, Int32.MaxValue, QueryTriggerInteraction.Ignore))
        {

            transform.position = hit.point - (camDir * m_decolliderSphereRadius);
            m_currentDistance = (hit.point - m_parent.position).magnitude - m_decolliderSphereRadius;
        }
    }

    void HandleRotation()
    {
        Vector2 input = InputManager.ReadLookValue();

        m_targetYaw += input.x * m_mouseSensitivity * Time.deltaTime;
        m_targetPitch -= input.y * m_mouseSensitivity * Time.deltaTime;
        m_targetPitch = Mathf.Clamp(m_targetPitch, -m_pitchClamp, m_pitchClamp);

        m_currentYaw = Mathf.SmoothDampAngle(m_currentYaw, m_targetYaw, ref m_yawVelocity, m_rotationSmoothTime);

        m_currentPitch = Mathf.SmoothDampAngle(m_currentPitch, m_targetPitch, ref m_pitchVelocity, m_rotationSmoothTime);
    }

    void HandleZoom()
    {
        float zoomInput = 0f;

        if (InputManager.CameraZoomIn())
        {
            zoomInput = -1f;
        }
        else if (InputManager.CameraZoomOut())
        {
            zoomInput = 1f;
        }

        m_targetDistance += zoomInput * m_zoomSpeed * Time.deltaTime;
        
        m_targetDistance = Mathf.Clamp(m_targetDistance, m_minZoomDistance, m_maxZoomDistance);

        m_currentDistance = Mathf.Lerp(m_currentDistance, m_targetDistance, m_zoomSmoothSpeed * Time.deltaTime);
    }

    void HandleShoulderSwitch()
    {
        if (InputManager.CameraChangeShoulder()) 
        {
            m_isRightShoulder = !m_isRightShoulder;
        }
    }

    void UpdateCamera()
    {
        Quaternion rotation = Quaternion.Euler(m_currentPitch, m_currentYaw, 0);

        Vector3 m_basePosition = m_parent.position + (m_thirdPersonCameraHeight * Vector3.up) + rotation * Vector3.back * m_currentDistance;

        float zoomT = 0f;

        if (m_currentDistance <= m_shoulderDisplacementStartDistance)
        {
            zoomT = 1f - Mathf.InverseLerp(m_minZoomDistance, m_shoulderDisplacementStartDistance, m_currentDistance);
        }
        if(m_currentDistance <= m_minZoomDistance)
        {
            zoomT = 1f - Mathf.InverseLerp(m_minZoomDistance, 0, m_currentDistance);
        }

        float targetShoulder = zoomT * (m_isRightShoulder ? 1f : -1f);

        m_currentShoulderOffset = Mathf.Lerp(m_currentShoulderOffset, targetShoulder, m_shoulderSmoothSpeed * Time.deltaTime);

        transform.rotation = rotation;

        Vector3 shoulderOffset = rotation * Vector3.right * m_currentShoulderOffset;

        transform.position = m_basePosition + shoulderOffset;        
    }
}