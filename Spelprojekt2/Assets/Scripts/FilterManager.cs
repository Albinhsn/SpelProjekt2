using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public struct FilterAction
{
    // NOTE(ah): _kind_ is redundant, can be infered by the index
    public FilterKind m_kind;
    public InputActionReference m_action;
}

public class FilterManager : MonoBehaviour
{
    [SerializeField]
    private FilterManagerData m_actionData;

    private int[] m_collidingWithCount;

    [SerializeField]
    public UnityEvent<FilterKind, bool> m_filterChanged;

    public static FilterKind m_activeFilter = FilterKind.None;

    void OnEnable()
    {
        Awake();
    }

    void Awake()
    {
        // ah: Enable all actions
        if(m_actionData.m_actions != null)
        {
            FilterAction[] actions = m_actionData.m_actions;
            for(int i = 0; i < actions.Length; i++)
            {
                actions[i].m_action.action.Enable();
            }
        }
        m_collidingWithCount = new int[(int)FilterKind.COUNT];
    }

    private bool CanDeactivateCurrentFilter()
    {
        bool result = true;
        if(m_activeFilter != FilterKind.None)
        {
            result = m_collidingWithCount[(int)m_activeFilter] == 0;
        }
        return result;
    }

    public void SetCollisionEnterWithFilter(FilterKind kind)
    {
        m_collidingWithCount[(int)kind]++;
    }

    public void SetCollisionLeaveWithFilter(FilterKind kind)
    {
        m_collidingWithCount[(int)kind]--;
    }

    void Update()
    {
        if(m_actionData.m_actions != null && CanDeactivateCurrentFilter())
        {
            // ah: check if any filter was activated
            FilterAction[] actions = m_actionData.m_actions;
            for(int i = 0; i < actions.Length; i++)
            {
                if(actions[i].m_action.action.WasPressedThisFrame())
                {
                    // ah: broadcast event
                    // HACK(ah): Currently player at least relies on this happening before 
                    // activating/deactivating objects. Because both rely on setting 
                    // _isKinematic_ to either true/false. So correct behaviour is 
                    // to set it to true (which activating does afterwards)
                    bool activating       = m_activeFilter != (FilterKind)i;
                    FilterKind new_filter = (FilterKind)i;
                    this.m_filterChanged?.Invoke(new_filter, activating);

                    // ah: activate objects
                    FilterObject[] objects = FindObjectsByType<FilterObject>(FindObjectsSortMode.None);
                    for(int j = 0; j < objects.Length; j++)
                    {
                        if(objects[j].m_kind == new_filter)
                        {
                            if(activating)
                            {
                                objects[j].Activate();
                            }
                            else
                            {
                                objects[j].Deactivate();
                            }
                        }
                    }

                    // ah: Change active index
                    if(activating)
                    {
                        if(m_activeFilter != FilterKind.None)
                        {
                            for(int j = 0; j < objects.Length; j++)
                            {
                                if(objects[j].m_kind == m_activeFilter)
                                {
                                    objects[j].Deactivate();
                                }
                            }
                        }
                        m_activeFilter = (FilterKind)i;
                    }
                    else
                    {
                        m_activeFilter = FilterKind.None;
                    }


                }
            }
        }
    }
}
