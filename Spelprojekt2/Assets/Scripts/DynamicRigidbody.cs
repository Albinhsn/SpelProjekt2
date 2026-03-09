using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class DynamicRigidbody : MonoBehaviour
{
    private Collider m_col;
    private Rigidbody m_rb;
    private bool m_shouldDestroy;
    private Mesh m_mesh;
    // HACK(ah): I feel disgusted with myself doing this but i don't know a better solution
    [HideInInspector] public HashSet<GameObject> m_collidingWithSet;

    void Awake()
    {
        this.m_rb = GetComponent<Rigidbody>();
        this.m_col = GetComponent<Collider>();
        this.m_collidingWithSet = new();
        this.m_mesh = GetComponent<Mesh>();
    }

    void Start()
    {
        FilterManager manager = FindFirstObjectByType<FilterManager>();
        manager.m_filterChanged.AddListener(OnFilterChange);
    }


    void OnFilterChange(FilterKind kind, bool active)
    {
        if(this.m_collidingWithSet != null && this.m_collidingWithSet.Count > 0)
        {
            if(!active)
            {
                m_shouldDestroy = true;
            }
            else
            {
                // Shouldn't destroy if you're the newly active filter (assuming it's not none)
                FilterObject filter = GetComponent<FilterObject>();
                bool is_newly_filtered = filter != null && kind == filter.m_kind;

                if(!is_newly_filtered)
                {
                    m_shouldDestroy = !(this.m_collidingWithSet == null || this.m_collidingWithSet.Count == 0);
                }

                if(m_shouldDestroy)
                {

                    foreach(GameObject obj in m_collidingWithSet)
                    {
                        FilterObject filter_obj = obj.GetComponent<FilterObject>();
                        if(filter_obj != null)
                        {
                            m_shouldDestroy = filter_obj.m_kind != kind;
                        }

                        if(m_shouldDestroy)
                        {
                            break;
                        }
                    }
                }
            }
            m_rb.WakeUp();
        }

    }

    public void Destroy()
    {
        Material material = GetComponent<MeshRenderer>().material;
        ParticleManager.PlayParticleEffect(transform.position, transform.rotation, m_mesh, material, transform.localScale);
        Destroy(this.gameObject);
    }

    public void DestroyOnCollision()
    {
        if(this.m_collidingWithSet == null) return;
        if(this.m_collidingWithSet.Count > 0)
        {
            Destroy();
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
        if(this.m_collidingWithSet != null)
        {
            this.m_collidingWithSet.Remove(other.gameObject);
        }
    }

    void LateUpdate()
    {
        if(m_shouldDestroy)
        {
            Destroy();
        }
    }
}
