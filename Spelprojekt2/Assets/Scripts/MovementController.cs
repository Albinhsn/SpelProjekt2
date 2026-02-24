using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Collections;
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
    [SerializeField, Tooltip("The movement speed of the player")]
    private float m_speed;

    [SerializeField,Tooltip("The force applied in y axis when the player jumps")] 
    private float m_jumpForce = 5.0f;
    [SerializeField,Range(0.0f, 1.0f), Tooltip("The higher the value, the smoother the acceleration/deceleration(0 = instant, 1 = no movement)")]
    private float m_smoothAccelerationFactor = 0.1f;
    private Rigidbody m_rb;
    private bool m_isJumping;
    private PlayerCamera m_playerCamera;
    private Vector3 m_referenceVector = Vector3.zero;

    private float jumpCooldown = 0;

    void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
        m_playerCamera = GetComponentInChildren<PlayerCamera>();
        m_footstepInstance = RuntimeManager.CreateInstance(m_footstepSound.evt);
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
        }
    }

    void Update()
    {
        if(InputManager.Jumped() && !m_isJumping && jumpCooldown <= 0)
        {
            m_rb.linearVelocity += new Vector3(0, m_jumpForce, 0);
            m_isJumping = true;
            SfxDirector.PlayCue2(m_playerJumpSound, this.transform.position);
            jumpCooldown = 0.5f;
            m_footstepInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        m_rb.position += m_referenceVector * Time.deltaTime;
        jumpCooldown -= Time.deltaTime;
    }

    void PlayFootstepSound()
    {
        m_footstepInstance.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE state);
        if(state == FMOD.Studio.PLAYBACK_STATE.STOPPED)
        {
            FMOD.RESULT ok = m_footstepInstance.start();

        }
    }

    void FixedUpdate()
    {
        // Calculate movement direction
        Vector3 forward = Rejection(m_playerCamera.m_playerForward, new Vector3(0, 1, 0));
        Vector3 right   = Rejection(m_playerCamera.m_playerRight, new Vector3(0, 1, 0));
        Vector2 input   = InputManager.ReadMovementValue();
        Vector3 dir     = forward * input.y + right * input.x;

        // Normalize direction
        if(dir.sqrMagnitude > 0)
        {
            dir = dir.normalized;

            if(!m_isJumping)
            {
                PlayFootstepSound();
            }
        }

        // Apply movement
        m_rb.linearVelocity = Vector3.Lerp(new Vector3(
            dir.x * m_speed, //Input
            m_rb.linearVelocity.y,
            dir.z * m_speed
        ), m_rb.linearVelocity, m_smoothAccelerationFactor); //Acceleration
    }

    private void UpdateReferenceVector(Vector3 reference)
    {
        //m_rb.linearVelocity -= m_referenceVector;
        m_referenceVector = reference;
        //m_rb.linearVelocity += reference;
    }
}
