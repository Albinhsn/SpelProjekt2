using UnityEngine;
using UnityEngine.Events;
using static LinAlg.LinAlg;
using UnityEngine.InputSystem;
using AudioKit.FMOD;
using Unity.VisualScripting;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class Player : MonoBehaviour
{

    [SerializeField]
    private AudioCueSO m_pickupCue;

    [SerializeField]
    private InputActionReference m_pickupItemAction;
    [Header("Pickup Settings")]
    [SerializeField] float m_itemMaxSpeed = 1f;
    [SerializeField] float m_itemDistanceFromPlayer = 1.5f;
    [SerializeField] float m_itemFloatHeight = 1.5f;

    [SerializeField] float m_maxPickupDistance;

    private Transform m_pickedupItem;

    private Collider m_collider;
    private const int MAX_NUMBER_OF_RAYS = 3;

    [Header("Filter events")]
    [SerializeField]
    public UnityEvent<FilterKind> m_onTriggerEnterWithFilter;

    [SerializeField]
    public UnityEvent<FilterKind> m_onTriggerLeaveWithFilter;

    

    private Rigidbody m_rb;

    void OnValidate()
    {
        if(m_maxPickupDistance > m_itemDistanceFromPlayer*3)
        {
            Debug.LogWarning("'Max Pickup Distance' should not be higher than three times the 'Item Distance From Player', otherwise the item will be dropped immediately after picking it up");
        }
    }

    void Awake()
    {
        m_pickupItemAction.action.Enable();
        m_collider = GetComponent<Collider>();
        m_rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        FilterManager fm = FindFirstObjectByType<FilterManager>();
        fm.m_filterChanged.AddListener(this.DropItemIfPickedupItemIsFiltered);
        this.m_onTriggerEnterWithFilter.AddListener(fm.SetCollisionEnterWithFilter);
        this.m_onTriggerLeaveWithFilter.AddListener(fm.SetCollisionLeaveWithFilter);
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
        Vector3 direction = Rejection(this.transform.forward, new Vector3(0,1,0));
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

                            // Play pickup sound
                            SfxDirector.PlayCue2(m_pickupCue, hit.transform.position);

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
            Vector3 targetPos = this.transform.position + this.transform.forward * m_itemDistanceFromPlayer + new Vector3(0, m_itemFloatHeight, 0);
            Vector3 itemToPlayerDir = this.transform.position - m_pickedupItem.transform.position;
            Vector2 itemToPlayerDirXZ = new Vector2(itemToPlayerDir.x, itemToPlayerDir.z).normalized;
            Vector2 itemToPlayerDirXZNormal = new Vector2(-itemToPlayerDirXZ.y, itemToPlayerDirXZ.x);
            float itemToPlayerDist = itemToPlayerDir.magnitude;
            float itemToPlayerDistXZ = new Vector2(itemToPlayerDir.x, itemToPlayerDir.z).magnitude;
            float distanceToCircle = itemToPlayerDist - m_itemDistanceFromPlayer;
            float distanceToCircleXZ = itemToPlayerDistXZ - m_itemDistanceFromPlayer;
            Vector3 itemToTargetPosDir = m_pickedupItem.transform.position + (itemToPlayerDir * distanceToCircle);
            Vector3 directionToTargetFromCircle = targetPos - itemToTargetPosDir;
            float distanceToTargetFromItemXZ = new Vector2(targetPos.x - m_pickedupItem.transform.position.x, targetPos.z - m_pickedupItem.transform.position.z).magnitude;

            // if item is outside of the circle the value is negative, if it's inside, the value is positive
            float forwardDir = -Mathf.Sign(distanceToCircleXZ);
            float Dir = Mathf.Sign(Vector2.Dot(itemToPlayerDirXZNormal, new Vector2(directionToTargetFromCircle.x, directionToTargetFromCircle.z)));

            float angle = Mathf.Clamp(Mathf.Abs(distanceToCircleXZ) * 100f,0,45f);

            Vector3 targetVelocity = new Vector3(
                itemToPlayerDirXZNormal.x * Mathf.Cos(Mathf.Deg2Rad*forwardDir*Dir*angle) - itemToPlayerDirXZNormal.y * Mathf.Sin(Mathf.Deg2Rad*forwardDir*Dir*angle),
                targetPos.y - m_pickedupItem.transform.position.y,
                itemToPlayerDirXZNormal.x * Mathf.Sin(Mathf.Deg2Rad*forwardDir*Dir*angle) + itemToPlayerDirXZNormal.y * Mathf.Cos(Mathf.Deg2Rad*forwardDir*Dir*angle)
            );
            
            float speed = Mathf.Clamp(distanceToTargetFromItemXZ*distanceToTargetFromItemXZ*2, 0, m_itemMaxSpeed);
            float verticalSpeed = Mathf.Clamp(Mathf.Abs(targetPos.y - m_pickedupItem.transform.position.y * targetPos.y - m_pickedupItem.transform.position.y * 2), 0, m_itemMaxSpeed);
            Rigidbody rb = m_pickedupItem.gameObject.GetComponent<Rigidbody>();
            if(rb != null)
            {
                rb.linearVelocity = m_rb.linearVelocity;
                rb.linearVelocity += new Vector3(targetVelocity.x * Dir * speed, targetVelocity.y * verticalSpeed, targetVelocity.z * Dir * speed);
            }
            Vector3 dir = transform.position - m_pickedupItem.transform.position;
            if(dir.magnitude > m_itemDistanceFromPlayer * 3f)
            {
                DropItem();
            }
        }
    }
}
