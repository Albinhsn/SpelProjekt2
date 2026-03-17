using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using AudioKit.FMOD;

public class FilterManager : MonoBehaviour
{

    public static Color[] FilterColorColors = {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow
    };

    private int[] m_collidingWithCount;
    private AudioCueSO m_filterChangeCue;

    [SerializeField]
    public UnityEvent<FilterKind, bool> m_filterChanged;

    public static bool m_filterUnlocked     = true;
    public static FilterKind m_activeFilter = FilterKind.None;
    public FilterColorData m_filterColorData;
    public FilterMaterialData m_filterMaterialData;
    public float m_filterEffectDistance = 30.0f;

    private AudioCueSO m_filterOffCue;
    private AudioCueSO m_filterOnCue;

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
        m_filterOnCue        = Resources.Load("Audio/AC_FilterOn") as AudioCueSO;
        m_filterOffCue       = Resources.Load("Audio/AC_FilterOff") as AudioCueSO;
        m_filterChanged = new();

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
        // ah: filter objects
        {
            FilterObject[] objects = FindObjectsByType<FilterObject>(FindObjectsSortMode.None);
            for(int j = 0; j < objects.Length; j++)
            {
                objects[j].ChangeMaterialColor(m_filterColorData, m_filterMaterialData);
            }
        }
        // change material to filtered kind
        {
            ChangeMaterialToFilteredKind[] objects = FindObjectsByType<ChangeMaterialToFilteredKind>(FindObjectsSortMode.None);
            for(int j = 0; j < objects.Length; j++)
            {
                objects[j].ChangeMaterialColor(m_filterColorData, m_filterMaterialData);
            }
        }

        // Emissive color from filter
        {
            EmissionColorFromFilter[] objects = FindObjectsByType<EmissionColorFromFilter>(FindObjectsSortMode.None);
            for(int j = 0; j < objects.Length; j++)
            {
                objects[j].UpdateMaterialColor(m_activeFilter);
            }
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

    // NOTE(ah): Assumes it's possible to actually change filters!
    public void ChangeFilter(FilterKind new_filter)
    {

        bool activating       = m_activeFilter != new_filter;


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
            for(int j = 0; j < objects.Length; j++)
            {
                if(objects[j].m_kind == m_activeFilter)
                {
                    objects[j].Deactivate();
                }
            }
        }

        // ah: play sfx
        {
            SfxDirector.PlayCue2(activating ? m_filterOnCue : m_filterOffCue, this.transform.position);
        }

        // ah: set global params
        {
            var audio_system = AudioSystem.I;
            if(audio_system != null)
            {
                switch(new_filter)
                {
                    case FilterKind.Primary:
                    {
                        audio_system.SetGlobalParam("PrimaryFilter", activating ? 1.0f : 0.0f);
                        break;
                    }
                    case FilterKind.Secondary:
                    {
                        audio_system.SetGlobalParam("SecondaryFilter", activating ? 1.0f : 0.0f);
                        break;
                    }
                }
                if(new_filter != m_activeFilter)
                {
                    switch(m_activeFilter)
                    {
                        case FilterKind.Primary:
                        {
                            audio_system.SetGlobalParam("PrimaryFilter", !activating ? 1.0f : 0.0f);
                            break;
                        }
                        case FilterKind.Secondary:
                        {
                            audio_system.SetGlobalParam("SecondaryFilter", !activating ? 1.0f : 0.0f);
                            break;
                        }
                    }
                }
                bool wasAnyFilterActive = m_activeFilter != FilterKind.None;
                FilterKind resultingFilter = activating ? new_filter : FilterKind.None;
                bool isAnyFilterActive = resultingFilter != FilterKind.None;

                if (wasAnyFilterActive != isAnyFilterActive)
                {
                    audio_system.SetSnapshot("snapshot:/FilterActive", isAnyFilterActive, 1f);
                }
            }

        }


        // ah: broadcast event
        this.m_filterChanged?.Invoke(new_filter, activating);

        m_activeFilter = activating ? new_filter : FilterKind.None;

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
        Player player = FindFirstObjectByType<Player>();
        if(player != null)
        {
            for(int j = 0; j < objects.Length; j++)
            {
                float distanceToPlayer = Vector3.Distance(objects[j].transform.position, player.transform.position);
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
}
