using UnityEngine;
using AudioKit.FMOD;
using FMODUnity;

public enum DoorDirection
{
    Up,
    Sideways,
    Forewards
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


    private Vector3 m_direction => direction == DoorDirection.Up ? new Vector3(0,1,0) : 
       direction == DoorDirection.Sideways ? new Vector3(1,0,0) : new Vector3(0,0,1);

    private float m_timeRemaining;
    private bool  m_closing;
    private bool  m_opening;
    private Vector3 m_closePosition;
    private bool m_isNotFiltered;

    private Collider m_collider;

    void Awake()
    {
        m_closePosition = this.transform.position;
        m_collider      = GetComponent<Collider>();

        m_isNotFiltered = IsNotFiltered();

        // Create sound instance and get id for freeze event
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

        m_soundInstance.setParameterByID(m_freezeID, val);
    }

    public void Open()
    {
        if(!m_opening)
        {
            m_opening = true;
            m_closing = false;

            float current_ratio = 0;
            if(direction == DoorDirection.Up)
            {
                current_ratio = 1.0f - (this.transform.position.y - m_closePosition.y) / m_openDistance;
            }
            if(direction == DoorDirection.Sideways)
            {
                current_ratio = 1.0f - (this.transform.position.x - m_closePosition.x) / m_openDistance;
            }
            if(direction == DoorDirection.Forewards)
            {
                current_ratio = 1.0f - (this.transform.position.z - m_closePosition.z) / m_openDistance;
            }
            m_timeRemaining = m_timeToOpen * current_ratio;

            SetSoundEventParam(DoorSoundState.Off);
        }
    }

    public void Close()
    {
        if(m_timeToClose != 0 && !m_closing)
        {
            m_closing = true;
            m_opening = false;
            float current_ratio = 0;
            if(direction == DoorDirection.Up)
            {
                current_ratio = (this.transform.position.y - m_closePosition.y) / m_openDistance;
            }
            if(direction == DoorDirection.Sideways)
            {
                current_ratio = (this.transform.position.x - m_closePosition.x) / m_openDistance;
            }
            if(direction == DoorDirection.Forewards)
            {
                current_ratio = (this.transform.position.z - m_closePosition.z) / m_openDistance;
            }
            m_timeRemaining = m_timeToClose * current_ratio;

            SetSoundEventParam(DoorSoundState.Off);
            AudioEventHub.I.SetEventParam(m_sound.evt.Path, "Off", 1.0f);
        }
    }

    // HACK(ah): This is a reaaaaly big hack but for now i'm out of ideas
    private bool IsNotFiltered()
    {
        return !m_collider.isTrigger;
    }


    void Update()
    {
        if(m_timeRemaining != 0)
        {
            if(IsNotFiltered())
            {
                m_timeRemaining = Mathf.Max(0, m_timeRemaining - Time.deltaTime);

                if(m_opening)
                {
                    float ratio       = 1.0f - m_timeRemaining / m_timeToOpen;
                    float additional_distance = Mathf.Lerp(0, m_openDistance, ratio);
                    this.transform.position = m_closePosition + m_direction * additional_distance;

                }
                else if(m_closing)
                {
                    float ratio = m_timeRemaining / m_timeToClose;
                    float additional_distance = Mathf.Lerp(0, m_openDistance, ratio);
                    this.transform.position = m_closePosition + m_direction * additional_distance;
                }
                else
                {
                    // ah: This should never happen
                    Debug.LogError("Invalid door state");
                    m_opening = false;
                    m_closing = false;
                    m_timeRemaining = 0;
                    this.transform.position = m_closePosition;
                }

                if(m_timeRemaining == 0)
                {
                    SetSoundEventParam(DoorSoundState.Freeze);
                    m_closing  = false;
                    m_opening  = false;
                }
                else if(!m_isNotFiltered)
                {
                    m_isNotFiltered = true;
                    SetSoundEventParam(DoorSoundState.Unfreeze);
                }
            }
            else
            {
                m_isNotFiltered = false;
                SetSoundEventParam(DoorSoundState.Freeze);
            }
        }
    }
}
