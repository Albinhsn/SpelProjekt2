using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public struct FilterAction
{
    // NOTE(ah): _kind_ is redundant, can be infered by the index
    public FilterKind kind;
    public InputActionReference action;
    // NOTE(ah): This needs to be an int because if we collide with 2 red filtered
    // objects it will basically be a raise condition if it's a bool. So instead
    // we just ref count it
    public int colliding_with_count;
}

public class FilterManager : MonoBehaviour
{
    [SerializeField]
    private FilterManagerData m_actionData;
    [SerializeField]
    private UnityEvent<FilterKind, bool> m_filterChanged;

    private FilterKind m_activeFilter = FilterKind.None;
    public List<List<FilterObject>> m_objects;


    public void SetCollisionEnterWithFilter(FilterKind kind)
    {
        m_actionData.m_actions[(int)kind].colliding_with_count++;
    }

    public void SetCollisionLeaveWithFilter(FilterKind kind)
    {
        m_actionData.m_actions[(int)kind].colliding_with_count--;
    }

    void Start()
    {
        // ah: Find each object that has FilterObject component and sort them by type.
        //
        // Doing this mostly to avoid having to when adding or changing an object both
        // adding to an list of events or changing it so that a designer can just change
        // the type and it just works.
        m_objects              = new List<List<FilterObject>>();
        FilterObject[] objects = FindObjectsByType<FilterObject>(FindObjectsSortMode.None);
        for(int i = 0; i < (int)FilterKind.COUNT; i++)
        {
            m_objects.Add(objects.Where(o => o.m_kind == (FilterKind)i).ToList());
        }

        // HACK(ah): We need to be able to not deactivate a filter if the player is colliding with
        // an object of that filter (i.e. player standing inside say a red plane).
        // So for now(?) just tag the filtered objects
        for(int i = 0; i < m_objects.Count; i++)
        {
            List<FilterObject> filter_objects = m_objects[i];
            string tag = ((FilterKind)i).ToString();
            for(int j = 0; j < filter_objects.Count; j++)
            {
                filter_objects[j].gameObject.tag = tag;
            }
        }

        // ah: Enable all actions
        if(m_actionData.m_actions != null)
        {
            FilterAction[] actions = m_actionData.m_actions;
            for(int i = 0; i < actions.Length; i++)
            {
                actions[i].action.action.Enable();
            }
        }

    }

    private bool CanDeactivateCurrentFilter()
    {
        bool result = true;
        if(m_activeFilter != FilterKind.None)
        {
            result = m_actionData.m_actions[(int)m_activeFilter].colliding_with_count == 0;
        }
        return result;
    }

    void Update()
    {
        if(m_actionData.m_actions != null && CanDeactivateCurrentFilter())
        {
            // ah: check if any filter was activated
            FilterAction[] actions = m_actionData.m_actions;
            for(int i = 0; i < actions.Length; i++)
            {
                if(actions[i].action.action.WasPressedThisFrame())
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
                    List<FilterObject> objects = m_objects[i];
                    for(int j = 0; j < objects.Count; j++)
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

                    // ah: Change active index
                    if(activating)
                    {
                        if(m_activeFilter != FilterKind.None)
                        {
                            List<FilterObject> current_objects = m_objects[(int)m_activeFilter];
                            for(int j = 0; j < current_objects.Count; j++)
                            {
                                current_objects[j].Deactivate();
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
