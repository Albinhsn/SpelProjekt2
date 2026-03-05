using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerCamera : MonoBehaviour
{
    [Header("Orbit")]
    [SerializeField] private float m_mouseSensitivity = 15f;
    [SerializeField] private float m_pitchAngleClamp = 60f;
    [SerializeField] private float m_rotationSmoothTime = 0.05f;
    [Header("Third Person")]
    [SerializeField] private float m_thirdPersonCameraHeight = .5f;
    [SerializeField] private float m_cameraDistanceFromPlayer = 3.5f;
    [SerializeField] private float m_distanceSmoothTime = .05f;
    [SerializeField] private float m_decolliderRadius = 1f;
    
    [Header("First Person/Zoom")]
    [SerializeField] private float m_firstPersonCameraHeight = .75f;
    [SerializeField] private float m_maxZoomDistance = 15;
    [SerializeField] private float m_zoomSensitivity = 50;
    [SerializeField] private float m_zoomSmoothSpeed = 10;
    [SerializeField] private float m_firstPersonRotationSmoothTime = .2f;
    [SerializeField] private float m_firstPersonDecolliderRadius = 1f;

    [Header("Shoulder")]
    [SerializeField] private float m_shoulderOffset = .8f;
    [SerializeField] private float m_shoulderSmoothSpeed = 8f;
    [Header("LayerMasks")]
    [SerializeField] private LayerMask m_layerToDecollideThirdPerson;
    [SerializeField] private LayerMask m_layerToDecollideFirstPerson;

    private float m_targetYaw;
    private float m_targetPitch;
    private float m_currentYaw;
    private float m_currentPitch;
    private float m_yawVelocity;
    private float m_pitchVelocity;
    private float m_distanceVelocity;
    private float m_targetDistance;
    private float m_currentDistance;
    private float m_currentShoulderOffset;
    private bool m_isRightShoulder = true;
    private Transform m_parent => transform.parent;
    private float m_savedPitch;
    private float m_savedYaw;
    private Vector3 m_basePosition;
    private bool m_changedToFirstPersonThisFrame = true;
    [HideInInspector] public Vector3 m_playerForward, m_playerRight;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        m_targetYaw = m_currentYaw = angles.y;
        m_targetPitch = m_currentPitch = angles.x;
        m_targetDistance = m_currentDistance = m_cameraDistanceFromPlayer;
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
        if(!m_changedToFirstPersonThisFrame)
        {
            m_changedToFirstPersonThisFrame = true;
            m_currentDistance = m_targetDistance = m_cameraDistanceFromPlayer;
            InputManager.EnablePlayerMovement();
        }
        HandleRotation(m_rotationSmoothTime);
        Decollider();
        
        UpdateCamera();
    }
    void FirstPersonCamera()
    {
        if(m_changedToFirstPersonThisFrame)
        {
            m_currentDistance = m_targetDistance = 0;
            m_changedToFirstPersonThisFrame = false;
            InputManager.DisablePlayerMovement();
        }

        HandleRotation(m_firstPersonRotationSmoothTime);
        HandleZoom();
        FirstPersonDecollider();

        Quaternion fpRotation = Quaternion.Euler(m_currentPitch, m_currentYaw, 0);

        transform.rotation = fpRotation;
        transform.position = m_parent.position + (Vector3.up * m_firstPersonCameraHeight) + fpRotation * Vector3.forward * m_currentDistance;
    }
    void HandleZoom()
    {
        float zoomInput = 0f;

        if (InputManager.CameraZoomIn())
        {
            zoomInput = 1f;
        }
        else if (InputManager.CameraZoomOut())
        {
            zoomInput = -1f;
        }

        m_targetDistance += zoomInput * m_zoomSensitivity * Time.deltaTime;
        
        m_targetDistance = Mathf.Clamp(m_targetDistance, 0, m_maxZoomDistance);
        m_currentDistance = Mathf.Lerp(m_currentDistance, m_targetDistance, m_zoomSmoothSpeed * Time.deltaTime);

    }
    void FirstPersonDecollider()
    {
        RaycastHit hit;
        if(Physics.Raycast(m_parent.position + (Vector3.up * m_firstPersonCameraHeight),transform.forward, out hit, m_targetDistance + m_firstPersonDecolliderRadius, m_layerToDecollideFirstPerson, QueryTriggerInteraction.Ignore))
        {
            m_targetDistance = (hit.point - m_parent.position).magnitude - m_firstPersonDecolliderRadius;
            m_currentDistance = m_targetDistance;
        }
    }
    void Decollider()
    {
        float sign = m_isRightShoulder ? 1 : -1;
        Vector3 m_baseDirection = (Vector3.back * m_cameraDistanceFromPlayer + sign * Vector3.right * m_shoulderOffset).normalized;

        Vector3 camDir = Quaternion.Euler(m_currentPitch, m_currentYaw, 0) * m_baseDirection;
        RaycastHit hit;
        
        if(Physics.Raycast(m_parent.position + (Vector3.up * m_thirdPersonCameraHeight), camDir, out hit, m_targetDistance + m_decolliderRadius, m_layerToDecollideThirdPerson, QueryTriggerInteraction.Ignore))
        {
            m_currentDistance = Mathf.SmoothDamp(m_currentDistance,(hit.point - m_parent.position).magnitude - m_decolliderRadius, ref m_distanceVelocity, m_distanceSmoothTime);
        }
        else
        {
            m_currentDistance = Mathf.SmoothDamp(m_currentDistance, m_cameraDistanceFromPlayer,ref m_distanceVelocity, m_distanceSmoothTime);
        }
    }

    void HandleRotation(float smoothFactor)
    {
        Vector2 input = InputManager.ReadLookValue();
        if(input != Vector2.zero)
        {
            m_targetYaw += input.x * m_mouseSensitivity * Time.deltaTime;
            m_targetPitch -= input.y * m_mouseSensitivity * Time.deltaTime;
        }

        m_targetPitch = Mathf.Clamp(m_targetPitch, -m_pitchAngleClamp, m_pitchAngleClamp);

        m_currentYaw = Mathf.SmoothDampAngle(m_currentYaw, m_targetYaw, ref m_yawVelocity, smoothFactor);

        m_currentPitch = Mathf.SmoothDampAngle(m_currentPitch, m_targetPitch, ref m_pitchVelocity, smoothFactor);
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
        
        m_basePosition = m_parent.position + (m_thirdPersonCameraHeight * Vector3.up) + rotation * Vector3.back * m_currentDistance;

        float distanceT = 1f - Mathf.InverseLerp(m_cameraDistanceFromPlayer, 0, m_currentDistance);        

        float targetShoulder = m_shoulderOffset * distanceT * (m_isRightShoulder ? 1f : -1f);

        m_currentShoulderOffset = Mathf.Lerp(m_currentShoulderOffset, targetShoulder, m_shoulderSmoothSpeed * Time.deltaTime);

        transform.rotation = rotation;

        Vector3 shoulderOffset = rotation * Vector3.right * m_currentShoulderOffset;

        transform.position = m_basePosition + shoulderOffset;        
    }
}