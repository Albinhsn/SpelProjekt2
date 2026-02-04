using UnityEngine;
using UnityEngine.Events;
using static LinAlg.LinAlg;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [SerializeField]
    private InputActionReference m_pickupItemAction;

    [SerializeField]
    private float m_maxPickupDistance;

    [SerializeField]
    [Range(0.1f, 4.0f)]
    private float m_maxDistancePickedupItemCanBeFromPlayer = 2.0f;

    [SerializeField]
    [Range(1.0f, 5.0f)]
    private float m_speedPickedupItemMovesTowardsPlayer = 3.0f;

    private Transform m_pickedupItem;
    private float m_pickedupItemDistance;

    private Collider m_collider;
    private const int MAX_NUMBER_OF_RAYS = 3;


    [Header("Filter events")]
    [SerializeField]
    private UnityEvent<FilterKind> m_onTriggerEnterWithFilter;

    [SerializeField]
    private UnityEvent<FilterKind> m_onTriggerLeaveWithFilter;

    private Rigidbody m_rb;

    void Awake()
    {
        m_pickupItemAction.action.Enable();
        m_collider = GetComponent<Collider>();
        m_rb = GetComponent<Rigidbody>();
    }

    void OnTriggerEnter(Collider other)
    {
        int filter_value = FilterObject.TagIsFilter(other.gameObject.tag);
        if(filter_value != -1)
        {
            m_onTriggerEnterWithFilter?.Invoke((FilterKind)filter_value);
        }
    }

    void OnTriggerExit(Collider other)
    {
        int filter_value = FilterObject.TagIsFilter(other.gameObject.tag);
        if(filter_value != -1)
        {
            m_onTriggerLeaveWithFilter?.Invoke((FilterKind)filter_value);
        }
    }

    public void DropItemIfPickedupItemIsFiltered(FilterKind kind, bool active)
    {
        if(active && m_pickedupItem != null)
        {
            if(m_pickedupItem.tag == kind.ToString())
            {
                DropItem();
            }
        }
    }

    void DropItem()
    {
        Rigidbody rb = m_pickedupItem.gameObject.GetComponent<Rigidbody>();
        if(rb != null)
        {
            rb.useGravity = true;
            rb.constraints = rb.constraints ^ RigidbodyConstraints.FreezeRotation;
        }

        m_pickedupItem = null;
    }

    struct RaycastResult
    {
        public bool hit;
        public RaycastHit data;
    };

    RaycastResult PickupRaycast(Vector3 origin)
    {
        RaycastResult result = new();
        Vector3 direction = Rejection(Camera.main.transform.forward, new Vector3(0,1,0));
        int layer_mask    = ~LayerMask.GetMask("Player");
        result.hit = Physics.Raycast(origin, direction, out result.data, Mathf.Infinity, layer_mask);
        return result;
    }


    void PickupItem()
    {

        if(m_pickedupItem != null)
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
                            m_pickedupItem = hit.transform;

                            // ah: transform the item to be infront of the player
                            Bounds hit_bounds = hit.collider.bounds;

                            Vector3 epsilon = new Vector3(0.1f, 0, 0.1f);
                            Vector3 offset = hit_bounds.extents + player_bounds.extents + epsilon;
                            offset.y = 0;
                            hit.transform.position = this.transform.position + Hadamard(offset, Rejection(Camera.main.transform.forward, new Vector3(0,1,0)).normalized);
                            hit.transform.rotation = this.transform.rotation;

                            // ah: store distance between them
                            m_pickedupItemDistance = (transform.position - hit.transform.position).magnitude;

                            // ah: Turn off rigid body
                            Rigidbody rb = hit.transform.gameObject.GetComponent<Rigidbody>();
                            if(rb != null)
                            {
                                rb.constraints = rb.constraints | RigidbodyConstraints.FreezeRotation;
                                rb.useGravity = false;
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
        if(m_pickupItemAction.action.WasPressedThisFrame())
        {
            PickupItem();
        }

        if(m_pickedupItem != null)
        {
            Rigidbody rb = m_pickedupItem.transform.gameObject.GetComponent<Rigidbody>();
            if(rb != null)
            {
                rb.linearVelocity = m_rb.linearVelocity;
            }

            Vector3 dir = this.transform.position - m_pickedupItem.transform.position;
            if(dir.magnitude > m_maxDistancePickedupItemCanBeFromPlayer)
            {
                DropItem();
            }

            // NOTE(ah): Some epsilon added so it's not to close when it tries to move
            const float DISTANCE_EPSILON = 0.3f;
            if(dir.magnitude > m_pickedupItemDistance + DISTANCE_EPSILON)
            {
                rb.linearVelocity += dir.normalized * m_speedPickedupItemMovesTowardsPlayer;
            }
        }
    }
}
