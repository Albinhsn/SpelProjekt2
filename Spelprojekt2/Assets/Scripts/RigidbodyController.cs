using UnityEngine;

public enum RigidbodyKind
{
    Static,
    Dynamic
}

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyController : MonoBehaviour
{
    [SerializeField]
    private RigidbodyKind kind;

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

    void OnTriggerEnter(Collider other)
    {
        FilterObject filter_obj = other.GetComponent<FilterObject>();
        if(filter_obj != null)
        {
            if(filter_obj.m_kind == FilterManager.m_activeFilter)
            {
                // TODO(ah): Do this only "within some threshold"
                collision_count++;
            }
        }
    }
}
