using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public class FilterManager : MonoBehaviour
{
    private int[] m_collidingWithCount;

    [SerializeField]
    public UnityEvent<FilterKind, bool> m_filterChanged;

    public static bool m_filterUnlocked     = true;
    public static FilterKind m_activeFilter = FilterKind.None;
    public FilterColorData m_filterColorData;
    public FilterMaterialData m_filterMaterialData;
    public float m_filterEffectDistance = 30.0f;

    public void Unlock()
    {
        m_filterUnlocked = true;
    }


    void OnEnable()
    {
        Awake();
    }

    void Awake()
    {
        m_filterMaterialData = Resources.Load("StandardFilterMaterialData") as FilterMaterialData;
        m_filterColorData    = Resources.Load("ScriptableObjects/FilterColorData") as FilterColorData;

        m_collidingWithCount = new int[(int)FilterKind.COUNT];

        ChangeFilterColor();
#if UNITY_EDITOR
            FilterManager.m_filterUnlocked = true;
#else
            FilterManager.m_filterUnlocked = false;
            #endif
    }

    public void ChangeFilterColor()
    {
        FilterObject[] objects = FindObjectsByType<FilterObject>(FindObjectsSortMode.None);
        for(int j = 0; j < objects.Length; j++)
        {
            objects[j].ChangeMaterialColor(m_filterColorData, m_filterMaterialData);
        }

        FilterDistanceChange distanceChangeObject = FindFirstObjectByType<FilterDistanceChange>();
        if(distanceChangeObject != null)
        {
            distanceChangeObject.ChangeMaterialColor(m_filterColorData, m_filterMaterialData);
        }
    }

    private bool CanDeactivateCurrentFilter()
    {
        bool result = true;
        if(!FilterManager.m_filterUnlocked)
        {
            result = false;
        }
        else if(m_activeFilter != FilterKind.None)
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

    public void ChangeFilter(FilterKind new_filter)
    {
        // NOTE(ah): Assumes it's possible to do!

        // ah: broadcast event
        // HACK(ah): Currently player at least relies on this happening before 
        // activating/deactivating objects. Because both rely on setting 
        // _isKinematic_ to either true/false. So correct behaviour is 
        // to set it to true (which activating does afterwards)
        bool activating       = m_activeFilter != new_filter;
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
            m_activeFilter = new_filter;
        }
        else
        {
            m_activeFilter = FilterKind.None;
        }

    }

    void Update()
    {
        if(CanDeactivateCurrentFilter())
        {
            // ah: check if any filter was activated
            for(int i = 0; i < (int)FilterKind.COUNT; i++)
            {
                if(InputManager.Filter((FilterKind)i))
                {
                    ChangeFilter((FilterKind)i);
                }
            }
        }

        FilterObject[] objects = FindObjectsByType<FilterObject>(FindObjectsSortMode.None);
        for(int j = 0; j < objects.Length; j++)
        {
            float distanceToPlayer = Vector3.Distance(objects[j].transform.position, FindFirstObjectByType<Player>().transform.position);
            if(distanceToPlayer > m_filterEffectDistance)
            {
                if(objects[j].Activated)
                {
                    objects[j].Deactivate();
                    
                    DynamicRigidbody[] rbs = FindObjectsByType<DynamicRigidbody>(FindObjectsSortMode.None);
                    for(int i = 0; i < rbs.Length; i++)
                    {
                        if(rbs[i].m_collidingWithSet == null) continue;
                        if(rbs[i].m_collidingWithSet.Contains(objects[j].gameObject))
                        {
                            rbs[i].DestroyOnCollision();
                        }
                    }

                    DynamicRigidbody rb = objects[j].GetComponent<DynamicRigidbody>();

                    if(rb != null)
                    {
                        rb.DestroyOnCollision();   
                    }
                }
            }
            else
            {
                if(objects[j].m_kind == m_activeFilter && !objects[j].Activated)
                {
                    objects[j].Activate();
                }
            }
        }
    }
}
