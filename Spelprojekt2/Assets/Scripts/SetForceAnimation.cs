using UnityEngine;

public class SetForceAnimation : MonoBehaviour
{
    [SerializeField] private Animator m_animator;
    private bool m_currentState;
    private int m_key;
    private bool m_valid;

    void Awake()
    {
        m_valid = false;
        for(int i = 0; m_animator != null && i < m_animator.parameters.Length; i++)
        {
            var param = m_animator.parameters[i];
            if(param.name == "Force")
            {
                m_key = param.nameHash;
                m_valid = true;
                break;
            }
        }

        if(!m_valid && m_animator != null)
        {
            Debug.LogError("Didn't find param \"Force\" in animator!");
        }
        else if(!m_valid)
        {
            Debug.LogError("SetForceAnimation on the player mesh needs an animator!");
        }
    }

    public void SetTrue()
    {
        if(m_valid && !m_currentState && m_animator != null)
        {
            m_animator.SetBool(m_key, true);
            m_currentState = true;
        }

    }

    public void SetFalse()
    {
        if(m_valid && m_currentState && m_animator != null)
        {
            m_animator.SetBool(m_key, false);
            m_currentState = false;
        }
    }
}
