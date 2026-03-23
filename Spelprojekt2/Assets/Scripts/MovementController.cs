using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using FMOD.Studio;
using AudioKit.FMOD;
using static LinAlg.LinAlg;

[RequireComponent(typeof(Rigidbody))]
public class MovementController : MonoBehaviour
{

    [Header("Sound")]
    [SerializeField]
    private AudioCueSO m_playerJumpSound;
    [SerializeField]
    private AudioCueSO m_footstepSound;
    private FMOD.Studio.EventInstance m_footstepInstance;

    [Header("Movement")]
    [SerializeField] private Animator m_animator;

    [SerializeField, Tooltip("The movement speed of the player while walking")]private float m_walkSpeed;
    [SerializeField, Tooltip("The movement speed of the player while sprinting")] private float m_sprintSpeed;
    [SerializeField, Tooltip("The movement influence the player has while in the air")] private float m_inAirSpeed;

    [SerializeField,Tooltip("The force applied in y axis when the player jumps")] 
    private float m_jumpForce = 5.0f;
    private float m_acceleration = 10;
    private Rigidbody m_rb;
    [HideInInspector] public bool m_isJumping;
    private PlayerCamera m_playerCamera;
    private Vector3 m_referenceVector = Vector3.zero;
    private Transform m_cameraTransform;

    private Dictionary<string, int> m_animationParameters;
    private Dictionary<string, bool> m_animationParamState;

    private float m_jumpCooldown = 0;
    // private bool m_sprinting = false;

    void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
        m_playerCamera = GetComponentInChildren<PlayerCamera>();

        m_footstepInstance = RuntimeManager.CreateInstance(m_footstepSound.evt);

        m_animationParameters = new();
        m_animationParamState = new();
        for(int i = 0; m_animator != null && i < m_animator.parameters.Length; i++)
        {
            var param = m_animator.parameters[i];
            m_animationParameters[param.name] = param.nameHash;
            m_animationParamState[param.name] = false;
        }
    }

    private void SetAnimTrigger(string key)
    {
        if(m_animationParameters.ContainsKey(key))
        {
            m_animator.SetTrigger(m_animationParameters[key]);
        }
    }

    private void SetAnimBool(string key, bool value)
    {
        if(m_animationParameters.ContainsKey(key) && m_animationParamState[key] != value)
        {
            m_animator.SetBool(m_animationParameters[key], value);
            m_animationParamState[key] = value;
        }
    }

    private void Start()
    {
        m_cameraTransform = Camera.main.transform;
    }

    void OnEnable()
    {
        Awake();
        Start();
    }

    private void OnDestroy()
    {
        m_footstepInstance.release();
    }

    private void OnDisable()
    {
        //Fix for continued movement while interacting (3rd or 4th time's the charm!)
        m_rb.linearVelocity = new Vector3(0, m_rb.linearVelocity.y, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!other.isTrigger)
        {
            m_isJumping = false;
            SetAnimBool("isGrounded", true);
            SetAnimBool("jumping", false);
        }
        if (other.TryGetComponent<MovingPlatformReceiver>(out MovingPlatformReceiver platform))
        {
            platform.d_onMovementChanged += UpdateReferenceVector;
            m_referenceVector = platform.reference;
        }
        else m_referenceVector = Vector3.zero;
    }

    private void OnTriggerExit(Collider other)
    {
        if(!other.isTrigger)
        {
            m_isJumping = true;
            SetAnimBool("isGrounded", false);
        }
        if (other.TryGetComponent<MovingPlatformReceiver>(out MovingPlatformReceiver platform))
        {
            platform.d_onMovementChanged -= UpdateReferenceVector;
        }
    }
    void OnTriggerStay(Collider other)
    {
        if(!other.isTrigger)
        {
            m_isJumping = false;
            SetAnimBool("isGrounded", true);
        }
    }

    void
    TryJump()
    {
        if(InputManager.Jumped() && !m_isJumping && m_jumpCooldown <= 0)
        {
            SetAnimBool("jumping", true);
            SetAnimBool("isGrounded", false);
            var vel = m_rb.linearVelocity;
            m_rb.linearVelocity = new Vector3(vel.x, m_jumpForce, vel.z);
            m_isJumping = true;
            SfxDirector.PlayCue2(m_playerJumpSound, this.transform.position);
            m_jumpCooldown = 0.5f;
            m_footstepInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }

    void Update()
    {
        TryJump();

        m_rb.position += m_referenceVector * Time.deltaTime;
        m_cameraTransform.position += m_referenceVector * Time.deltaTime;
        
        m_jumpCooldown -= Time.deltaTime;

        // if(InputManager.Sprinting())
        // {
        //     m_sprinting = true;
        // }
    }

    void PlayFootstepSound()
    {
        m_footstepInstance.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE state);
        if(state == FMOD.Studio.PLAYBACK_STATE.STOPPED)
        {
            FMOD.RESULT ok = m_footstepInstance.start();

        }
    }

    private float m_currentSpeed;
    void FixedUpdate()
    {
        // Calculate movement direction
        Vector3 forward = Rejection(m_playerCamera.m_playerForward, new Vector3(0, 1, 0));
        Vector3 right   = Rejection(m_playerCamera.m_playerRight, new Vector3(0, 1, 0));
        Vector2 input   = InputManager.ReadMovementValue();
        Vector3 dir     = forward * input.y + right * input.x;

        // TryJump();
        

        // Normalize direction
        if (dir.sqrMagnitude > 0)
        {
            dir = dir.normalized;

            if (!m_isJumping)
            {
                PlayFootstepSound();
            }
        }

        if(InputManager.Sprinting())
        {
            if (m_currentSpeed <= m_sprintSpeed)
            {
                m_currentSpeed = MathF.Min(m_sprintSpeed, m_currentSpeed + m_acceleration * Time.fixedDeltaTime);
            }
            else
            {
                m_currentSpeed = m_currentSpeed - m_acceleration * Time.fixedDeltaTime;
            }
        }
        else
        {
            if (m_currentSpeed <= m_walkSpeed)
            {
                m_currentSpeed = MathF.Min(m_walkSpeed, m_currentSpeed + m_acceleration * Time.fixedDeltaTime);
            }
            else
            {
                m_currentSpeed = m_currentSpeed - m_acceleration * Time.fixedDeltaTime;
            }
        }

        m_currentSpeed *= dir.magnitude;
        // Apply movement
        if(!m_isJumping)
        {
            m_rb.linearVelocity = m_referenceVector + new Vector3(
                dir.x * m_currentSpeed, //Input
                m_rb.linearVelocity.y,
                dir.z * m_currentSpeed);
        }
        else
        {
            m_rb.linearVelocity += new Vector3(
            dir.x * m_inAirSpeed, //Input
            0,
            dir.z *  m_inAirSpeed
            ); //Acceleration
            Vector3 xzVelocity = new Vector3(m_rb.linearVelocity.x,0,m_rb.linearVelocity.z);

            if(xzVelocity.magnitude > m_currentSpeed)
            {
                xzVelocity = xzVelocity.normalized * m_currentSpeed;
                m_rb.linearVelocity = new Vector3(xzVelocity.x,m_rb.linearVelocity.y,xzVelocity.z);
            }
        }
        
        m_animator.SetFloat("horizontalVelocity", new Vector2(m_rb.linearVelocity.x - m_referenceVector.x, m_rb.linearVelocity.z - m_referenceVector.z).magnitude);
        m_animator.SetFloat("verticalVelocity", m_rb.linearVelocity.y - m_referenceVector.y);
    }

    private void UpdateReferenceVector(Vector3 reference)
    {
        //m_rb.linearVelocity -= m_referenceVector;
        m_referenceVector = reference;
        //m_rb.linearVelocity += reference;
    }
}
