using UnityEngine;
using AudioKit.FMOD;
using FMODUnity;

public enum DoorDirection
{
    Up,
    Sideways,
    Forwards
}

[RequireComponent(typeof(Collider))]
public class Door : MonoBehaviour
{
    enum DoorSoundState
    {
        Freeze,
        Unfreeze,
        Off
    }

    [SerializeField]
    private Rigidbody m_rb;

    [SerializeField]
    private AudioCueSO m_sound;

    private FMOD.Studio.EventInstance m_soundInstance;
    private FMOD.Studio.PARAMETER_ID m_freezeID;

    [SerializeField]
    private float m_timeToOpen;

    [SerializeField]
    [Tooltip("Setting this to 0 will make the door never close")]
    private float m_timeToClose;

    [SerializeField]
    private float m_openDistance;

    [SerializeField]
    private DoorDirection direction; 

    private Vector3 m_direction => direction == DoorDirection.Up ? this.transform.up : 
       direction == DoorDirection.Sideways ? this.transform.right  : this.transform.forward;


    private bool  m_closing;
    private bool  m_opening;

    private Vector3 m_closePosition;
    private Vector3 m_openPosition;

    private Collider m_collider;

    private float m_openSpeed;
    private float m_closeSpeed;


    void Awake()
    {
        m_closePosition = this.transform.position;
        m_openPosition  = m_closePosition + m_direction * m_openDistance;
        m_collider      = GetComponent<Collider>();

        if(m_rb == null)
        {
            Debug.LogError("Door needs rigidbody!");
        }
        else
        {
            // ah: lock rotations 
            m_rb.constraints = RigidbodyConstraints.FreezeRotation;
            m_rb.useGravity  = false;

        }

        // Create sound instance and get id for freeze event
        if(m_sound != null)
        {
            m_soundInstance = RuntimeManager.CreateInstance(m_sound.evt);

            FMOD.Studio.EventDescription freezeEventDescription;
            m_soundInstance.getDescription(out freezeEventDescription);
            FMOD.Studio.PARAMETER_DESCRIPTION freezeParameterDescription;

            int count;
            freezeEventDescription.getParameterDescriptionCount(out count);
            FMOD.RESULT ok = freezeEventDescription.getParameterDescriptionByName("freeze", out freezeParameterDescription);
            m_freezeID = freezeParameterDescription.id;

            m_soundInstance.set3DAttributes(RuntimeUtils.To3DAttributes(this.transform.position));
            SetSoundEventParam(DoorSoundState.Freeze);
            m_soundInstance.start();
        }

        m_closeSpeed = -m_openDistance / m_timeToClose;
        m_openSpeed  = m_openDistance / m_timeToOpen;
    }

    void OnDestroy()
    {
        m_soundInstance.release();
    }

    private void SetSoundEventParam(DoorSoundState state)
    {
        float val = 0.0f;
        switch(state)
        {
            case DoorSoundState.Freeze:
            {
                val = 1.0f;
                break;
            }
            case DoorSoundState.Unfreeze:
            {
                val = 0.0f;
                break;
            }
            case DoorSoundState.Off:
            {
                val = 2.0f;
                break;
            }
        }

        if(m_sound != null)
        {
            m_soundInstance.setParameterByID(m_freezeID, val);
        }
    }

    public void Open()
    {
        bool is_fully_opened = Vector3.Dot(this.transform.position, m_direction * Mathf.Sign(m_openDistance)) >= Vector3.Dot(m_openPosition, m_direction * Mathf.Sign(m_openDistance));
        if(!m_opening && !is_fully_opened)
        {
            m_opening = true;
            m_closing = false;
            SetSoundEventParam(DoorSoundState.Off);
        }
    }

    public void Close()
    {
        bool is_fully_closed = Vector3.Dot(this.transform.position, -m_direction * Mathf.Sign(m_openDistance)) >= Vector3.Dot(m_closePosition, -m_direction * Mathf.Sign(m_openDistance));
        if(!m_closing && !is_fully_closed)
        {
            m_opening = false;
            m_closing = true;
            SetSoundEventParam(DoorSoundState.Off);
        }
    }

    // HACK(ah): This is a reaaaaly big hack but for now i'm out of ideas
    private bool IsNotFiltered()
    {
        return !m_collider.isTrigger;
    }


    private void SetVelocity(float speed)
    {
        if(m_rb != null && !m_rb.isKinematic) m_rb.linearVelocity = m_direction * speed;
    }

    void FixedUpdate()
    {
        bool is_not_moving = !IsNotFiltered() && (m_opening || m_closing);

        if(!is_not_moving)
        {
            if(m_opening)
            {
                is_not_moving = Vector3.Dot(this.transform.position, m_direction * Mathf.Sign(m_openDistance)) >= Vector3.Dot(m_openPosition, m_direction * Mathf.Sign(m_openDistance));
                if(is_not_moving)
                {
                    this.transform.position = m_openPosition;
                    m_opening = false;
                }
                else
                {
                    SetVelocity(m_openSpeed);
                }
            }
            else if(m_closing)
            {
                is_not_moving = Vector3.Dot(this.transform.position, -m_direction * Mathf.Sign(m_openDistance)) >= Vector3.Dot(m_closePosition, -m_direction * Mathf.Sign(m_openDistance));
                if(is_not_moving)
                {
                    this.transform.position = m_closePosition;
                    m_closing = false;
                }
                else
                {
                    SetVelocity(m_closeSpeed);
                }
            }
        }

        if(is_not_moving)
        {
            SetVelocity(0);
            SetSoundEventParam(DoorSoundState.Freeze);
        }
    }
}
