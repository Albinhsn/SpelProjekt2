using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Door : MonoBehaviour
{
    [SerializeField]
    private float m_timeToOpen;

    [SerializeField]
    [Tooltip("Setting this to 0 will make the door never close")]
    private float m_timeToClose;

    [SerializeField]
    private float m_openHeight;


    private float m_timeRemaining;
    private bool  m_closing;
    private bool  m_opening;
    private Vector3 m_closePosition;

    private Collider m_collider;

    void Awake()
    {
        m_closePosition = this.transform.position;
        m_collider      = GetComponent<Collider>();
    }

    public void Open()
    {
        if(!m_opening)
        {
            // this.transform.position = m_closePosition + new Vector3(0, m_openHeight, 0);
            m_opening = true;
            m_closing = false;
            float current_height_ratio = 1.0f - (this.transform.position.y - m_closePosition.y) / m_openHeight;
            m_timeRemaining = m_timeToOpen * current_height_ratio;
        }
    }

    public void Close()
    {
        if(m_timeToClose != 0 && !m_closing)
        {

            m_closing = true;
            m_opening = false;

            float current_height_ratio = (this.transform.position.y - m_closePosition.y) / m_openHeight;
            m_timeRemaining = m_timeToClose * current_height_ratio;
        }
    }

    // HACK(ah): This is a reaaaaly big hack but for now i'm out of ideas
    private bool IsNotFiltered()
    {
        return m_collider.enabled;
    }

    void Update()
    {
        if(m_timeRemaining != 0 && IsNotFiltered())
        {
            m_timeRemaining = Mathf.Max(0, m_timeRemaining - Time.deltaTime);

            if(m_opening)
            {
                float heightRatio       = 1.0f - m_timeRemaining / m_timeToOpen;
                float additional_height = Mathf.Lerp(0, m_openHeight, heightRatio);
                this.transform.position = m_closePosition + new Vector3(0, additional_height, 0);

            }
            else if(m_closing)
            {
                float heightRatio       = m_timeRemaining / m_timeToClose;
                float additional_height = Mathf.Lerp(0, m_openHeight, heightRatio);
                this.transform.position = m_closePosition + new Vector3(0, additional_height, 0);
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
                m_closing  = false;
                m_opening  = false;
            }


        }
    }
}
