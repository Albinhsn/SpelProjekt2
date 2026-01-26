using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Setting this to 0 will make the door never close")]
    private float m_timeToOpen;

    [SerializeField]
    private float m_openHeight;


    private float m_timeRemaining;
    private bool  m_closing;
    private bool  m_opened;
    private Vector3 closePosition;

    void Awake()
    {
        closePosition = this.transform.position;
    }

    public void Open()
    {
        if(!m_opened)
        {
            this.transform.position = closePosition + new Vector3(0, m_openHeight, 0);
            m_opened = true;
        }
    }

    public void Close()
    {
        if(m_timeToOpen != 0 && !m_closing)
        {
            m_closing = true;
            m_timeRemaining = m_timeToOpen;
        }
    }

    void Update()
    {
        if(m_timeRemaining != 0)
        {
            m_timeRemaining = Mathf.Max(0, m_timeRemaining - Time.deltaTime);
            float heightRatio = m_timeRemaining / m_timeToOpen;
            float additional_height = Mathf.Lerp(0, m_openHeight, heightRatio);
            this.transform.position = closePosition + new Vector3(0, additional_height, 0);

            if(m_timeRemaining == 0)
            {
                m_closing = false;
                m_opened  = false;
            }
        }
    }
}
