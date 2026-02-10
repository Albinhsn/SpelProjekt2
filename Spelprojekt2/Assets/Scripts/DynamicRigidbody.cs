using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DynamicRigidbody : MonoBehaviour
{
    private int collision_count;

    void Start()
    {
        FilterManager manager = FindFirstObjectByType<FilterManager>();
        manager.m_filterChanged.AddListener(OnFilterChange);
    }


    void OnFilterChange(FilterKind kind, bool active)
    {
        if(!active && collision_count > 0)
        {
            Destroy(this.gameObject);
        }
        else
        {
            collision_count = 0;
        }

    }

    bool ColliderOrSelfIsFiltered(Collider other)
    {
        bool is_filtered = false;
        // Check other
        {
            FilterObject filter_obj = other.GetComponent<FilterObject>();
            if(filter_obj != null)
            {
                is_filtered |= filter_obj.m_kind == FilterManager.m_activeFilter;
            }
        }

        // Check self
        {
            FilterObject filter_obj = this.GetComponent<FilterObject>();
            if(filter_obj != null)
            {
                is_filtered |= filter_obj.m_kind == FilterManager.m_activeFilter;
            }
        }

        return is_filtered;
    }

    void OnTriggerEnter(Collider other)
    {
        if(ColliderOrSelfIsFiltered(other))
        {
            collision_count++;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(ColliderOrSelfIsFiltered(other))
        {
            collision_count--;
        }
    }
}
