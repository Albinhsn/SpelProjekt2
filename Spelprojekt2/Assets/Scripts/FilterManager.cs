using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public struct FilterAction
{
    // NOTE(ah): _kind_ is redundant, can be infered by the index
    public FilterKind kind;
    public InputActionReference action;
}

public class FilterManager : MonoBehaviour
{
    [SerializeField]
    private FilterManagerData m_actionData;


    private int m_activeIndex = -1;
    public List<List<FilterObject>> m_objects;

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

    void Update()
    {
        if(m_actionData.m_actions != null)
        {
            // ah: check if any filter was activated
            FilterAction[] actions = m_actionData.m_actions;
            for(int i = 0; i < actions.Length; i++)
            {
                if(actions[i].action.action.WasPressedThisFrame())
                {
                    // ah: activate objects
                    List<FilterObject> objects = m_objects[i];
                    for(int j = 0; j < objects.Count; j++)
                    {
                        if(m_activeIndex != i)
                        {
                            objects[j].Activate();
                        }
                        else
                        {
                            objects[j].Deactivate();
                        }
                    }

                    // ah: Change active index
                    if(m_activeIndex != i)
                    {
                        if(m_activeIndex != -1)
                        {
                            List<FilterObject> current_objects = m_objects[m_activeIndex];
                            for(int j = 0; j < current_objects.Count; j++)
                            {
                                current_objects[j].Deactivate();
                            }
                        }
                        m_activeIndex = i;
                    }
                    else
                    {
                        m_activeIndex = -1;
                    }
                }
            }
        }
    }
}
