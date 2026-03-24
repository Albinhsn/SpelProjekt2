using UnityEngine;

public class AnimatePosition : MonoBehaviour
{
    [SerializeField]
    private Vector3 m_delta;

    [SerializeField]
    private float m_speed;

    private float m_animationTime;
    private float m_t;
    private float dir;
    private Vector3 m_spawnP;

    void Awake()
    {
        m_animationTime = m_delta.magnitude / m_speed;
        dir = 1;
        m_spawnP = this.transform.position;
    }

    void FixedUpdate()
    {
        m_animationTime = m_delta.magnitude / m_speed;
        if(m_animationTime > 0)
        {
            m_t += dir * Time.fixedDeltaTime;
            if(m_t <= 0)
            {
                m_t = 0;
                dir = 1;
            }
            if(m_t >= m_animationTime)
            {
                dir = -1;
                m_t = m_animationTime;
            }
            this.transform.position = m_spawnP + m_delta * (m_t / m_animationTime);
        }

    }
}
