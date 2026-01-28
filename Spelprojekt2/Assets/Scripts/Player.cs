using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class Player : MonoBehaviour
{
    [SerializeField]
    private InputActionReference m_pickupItemAction;

    [SerializeField]
    private float m_maxPickupDistance;

    private Transform pickedup_item;

    private Collider m_collider;
    private const int MAX_NUMBER_OF_RAYS = 3;


    [Header("Filter events")]
    [SerializeField]
    private UnityEvent<FilterKind> m_onTriggerEnterWithFilter;

    [SerializeField]
    private UnityEvent<FilterKind> m_onTriggerLeaveWithFilter;


    private int TagIsFilter(string tag)
    {
        for(int i = 0; i < (int)FilterKind.COUNT; i++)
        {
            if(tag == ((FilterKind)i).ToString())
            {
                return i;
            }
        }
        return -1;
    }

    void Awake()
    {
        m_pickupItemAction.action.Enable();
        m_collider = GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider other)
    {
        int filter_value = TagIsFilter(other.gameObject.tag);
        if(filter_value != -1)
        {
            m_onTriggerEnterWithFilter?.Invoke((FilterKind)filter_value);
        }
    }

    void OnTriggerExit(Collider other)
    {
        int filter_value = TagIsFilter(other.gameObject.tag);
        if(filter_value != -1)
        {
            m_onTriggerLeaveWithFilter?.Invoke((FilterKind)filter_value);
        }
    }

    public void DropItemIfPickedupItemIsFiltered(FilterKind kind, bool active)
    {
        if(active && pickedup_item != null)
        {
            if(pickedup_item.tag == kind.ToString())
            {
                DropItem();
            }
        }
    }

    void DropItem()
    {
        // ah: drop item
        pickedup_item.SetParent(null);

        Rigidbody rb = pickedup_item.gameObject.GetComponent<Rigidbody>();
        if(rb != null)
        {
            rb.isKinematic = false;
        }

        pickedup_item = null;
    }

    struct RaycastResult
    {
        public bool hit;
        public RaycastHit data;
    };

    RaycastResult PickupRaycast(Vector3 origin)
    {
        RaycastResult result = new();
        Vector3 direction = this.transform.forward;
        int layer_mask    = ~LayerMask.GetMask("Player");
        result.hit = Physics.Raycast(origin, direction, out result.data, Mathf.Infinity, layer_mask);
        return result;
    }


    void PickupItem()
    {

        if(pickedup_item != null)
        {
            DropItem();
        }
        else
        {
            // ah: try to find item to pick up
            Vector3 player_p = this.transform.position;

            Bounds player_bounds = this.m_collider.bounds;
            Vector3 origin = player_p;
            float bounds_half_extent_height = player_bounds.extents.y * 0.5f;
            origin.y -= bounds_half_extent_height;
            for(int i = 0; i < MAX_NUMBER_OF_RAYS; i++)
            {
                RaycastResult result = PickupRaycast(origin);
                RaycastHit hit = result.data;
                if(result.hit)
                {
                    int pickable_mask = LayerMask.GetMask("Pickable");
                    if(hit.distance < m_maxPickupDistance)
                    {
                        // ah: check if item is filtered already
                        bool is_pickable = ((1 << hit.transform.gameObject.layer) & pickable_mask) != 0;

                        bool is_not_filtered = true;
                        FilterObject filter_object = hit.transform.gameObject.GetComponent<FilterObject>();
                        if(filter_object != null)
                        {
                            is_not_filtered = !filter_object.Activated;
                        }

                        if(is_pickable && is_not_filtered)
                        {
                            // ah: pick up the item
                            pickedup_item = hit.transform;

                            // ah: transform the item to be infront of the player
                            Bounds hit_bounds = hit.collider.bounds;

                            Vector3 epsilon = new Vector3(0.1f, 0, 0.1f);
                            Vector3 offset = hit_bounds.extents + player_bounds.extents + epsilon;
                            offset.y = 0;
                            hit.transform.position = this.transform.position + MovementController.Hadamard(offset, this.transform.forward);
                            hit.transform.rotation = this.transform.rotation;
                            hit.transform.SetParent(this.transform);

                            // ah: Turn off rigid body
                            Rigidbody rb = hit.transform.gameObject.GetComponent<Rigidbody>();
                            if(rb != null)
                            {
                                rb.isKinematic = true;
                            }
                        }
                    }
                    break;
                }
                origin.y += bounds_half_extent_height;
            }
        }
    }

    void Update()
    {
        Debug.DrawRay(this.transform.position, this.transform.forward);
        if(m_pickupItemAction.action.WasPressedThisFrame())
        {
            PickupItem();
        }

    }


}
