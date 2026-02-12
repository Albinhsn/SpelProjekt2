using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class DynamicRigidbody : MonoBehaviour
{
    private Collider m_col;
    // HACK(ah): I feel disgusted with myself doing this but i don't know a better solution
    private HashSet<GameObject> m_collidingWithSet;

    void Start()
    {
        FilterManager manager = FindFirstObjectByType<FilterManager>();
        manager.m_filterChanged.AddListener(OnFilterChange);
        this.m_col = GetComponent<Collider>();

        this.m_collidingWithSet = new();
    }


    void OnFilterChange(FilterKind kind, bool active)
    {
        if(!active && this.m_collidingWithSet.Count > 0)
        {
            Destroy(this.gameObject);
        }
        else
        {
            this.m_collidingWithSet.Clear();
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

    void OnTriggerStay(Collider other)
    {
        if(other.isTrigger && other.gameObject.GetComponent<FilterObject>() == null)
        {
            return;
        }
        float EPSILON = 0.05f;
        if(ColliderOrSelfIsFiltered(other))
        {

            Vector3 other_pos = other.gameObject.transform.position;
            Quaternion other_rot = other.gameObject.transform.rotation;

            Vector3 dir;
            float distance;
            if(Physics.ComputePenetration(this.m_col, this.transform.position, this.transform.rotation,
                        other, other_pos, other_rot, out dir, out distance))
            {
                if(distance > EPSILON)
                {
                    this.m_collidingWithSet.Add(other.gameObject);
                }
                else
                {
                    this.m_collidingWithSet.Remove(other.gameObject);
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        this.m_collidingWithSet.Remove(other.gameObject);
    }
}
