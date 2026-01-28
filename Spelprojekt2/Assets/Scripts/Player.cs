using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{

    [SerializeField]
    private UnityEvent<FilterKind> m_onTriggerEnterWithFilter;

    [SerializeField]
    private UnityEvent<FilterKind> m_onTriggerLeaveWithFilter;

    private int TagIsFilter(string tag)
    {
        for(int i = 0; i < (int)FilterKind.COUNT; i++)
        {
            if(tag == ((FilterKind)i).ToString())
            {
                return i;
            }
        }
        return -1;
    }

    void OnTriggerEnter(Collider other)
    {
        int filter_value = TagIsFilter(other.gameObject.tag);
        if(filter_value != -1)
        {
            m_onTriggerEnterWithFilter?.Invoke((FilterKind)filter_value);
        }
    }

    void OnTriggerExit(Collider other)
    {
        int filter_value = TagIsFilter(other.gameObject.tag);
        if(filter_value != -1)
        {
            m_onTriggerLeaveWithFilter?.Invoke((FilterKind)filter_value);
        }
    }
}
