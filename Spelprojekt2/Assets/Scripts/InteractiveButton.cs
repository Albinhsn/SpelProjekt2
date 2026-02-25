using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System.Collections.Generic;
using AudioKit.FMOD;


public class InteractiveButton : MonoBehaviour
{
    [Header("Sound")]
    [SerializeField]
    private string m_onClickSoundEvent;

    [Header("Events")]
    [SerializeField]
    private UnityEvent m_onClick;
    [SerializeField]
    private UnityEvent m_onRelease;

    private List<GameObject> m_collidingObjects;


    void Start()
    {
        m_collidingObjects = new();
        FilterManager fm = FindFirstObjectByType<FilterManager>();
        fm.m_filterChanged.AddListener(HandleFilterChange);
    }

    bool IsNoObjectOnTheButton(FilterKind kind)
    {
        bool result = true;
        for(int i = 0; i < m_collidingObjects.Count; i++)
        {
            if(m_collidingObjects[i] == null)
            {
                m_collidingObjects.RemoveAt(i);
                i--;
                continue;
            }
            FilterObject obj = m_collidingObjects[i].GetComponent<FilterObject>();

            if(obj != null)
            {
                if(kind != obj.m_kind)
                {
                    result = false;
                    break;
                }
            }
            else
            {
                result = false;
                break;
            }
        }
        return result;
    }

    void Update()
    {
        if(m_collidingObjects.Count > 0)
        {
            for(int i = 0; i < m_collidingObjects.Count; i++)
            {
                if(m_collidingObjects[i] == null)
                {
                    m_collidingObjects.RemoveAt(i);
                    i--;
                    if(IsNoObjectOnTheButton(FilterManager.m_activeFilter))
                    {
                        m_onRelease?.Invoke();
                    }
                }
            }
        }
    }

    private void Click()
    {
        m_onClick?.Invoke();
        AudioEventHub.I?.PlayOneShot(m_onClickSoundEvent, this.transform.position);
    }

    private void Release()
    {
        m_onRelease?.Invoke();
    }


    private void OnTriggerEnter(Collider other)
    {
        FilterObject obj = other.gameObject.GetComponent<FilterObject>();
        bool is_valid_object = true;
        if(obj != null)
        {
            is_valid_object = obj.m_kind != FilterManager.m_activeFilter;
        }

        if(is_valid_object && IsNoObjectOnTheButton(FilterManager.m_activeFilter))
        {
            Click();
        }
        m_collidingObjects.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        FilterObject obj = other.gameObject.GetComponent<FilterObject>();
        bool is_valid_object = true;
        if(obj != null)
        {
            is_valid_object = obj.m_kind != FilterManager.m_activeFilter;
        }

        if(is_valid_object)
        {
            m_collidingObjects = m_collidingObjects.Where(x => x != other.gameObject).ToList();
            if(IsNoObjectOnTheButton(FilterManager.m_activeFilter))
            {
                Release();
            }
        }
    }

    public void HandleFilterChange(FilterKind kind, bool activate)
    {

        bool object_on_btn_before_change = IsNoObjectOnTheButton(FilterManager.m_activeFilter);
        bool object_on_btn_after_change  = IsNoObjectOnTheButton(activate ? kind : FilterKind.None);
        if(object_on_btn_before_change  && !object_on_btn_after_change)
        {
            m_onClick?.Invoke();
        }

        if(!object_on_btn_before_change  && object_on_btn_after_change)
        {
            m_onRelease?.Invoke();
        }
    }

}
