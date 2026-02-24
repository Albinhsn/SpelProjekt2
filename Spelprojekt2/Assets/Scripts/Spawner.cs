using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject m_objectToSpawn;
    [SerializeField] private Vector3 m_initialVelocity;

    [SerializeField]
    private bool m_spawnOnActivation;

    private GameObject m_object;
    
    
    public void Awake()
    {
        if(!m_spawnOnActivation)
        {
            Spawn();
        }
    }

    void OnDestroy()
    {
        if(m_object != null)
        {
            Destroy(m_object);
        }
    }

    public bool CanSpawn()
    {
        bool result = true;
        if(m_object != null)
        {
            FilterObject filter_object = m_object.GetComponent<FilterObject>();
            if(filter_object != null)
            {
                result = filter_object.m_kind != FilterManager.m_activeFilter;
            }

        }
        return result;
    }

    public void Spawn()
    {
        if(CanSpawn())
        {
            Despawn();
            this.m_object = Instantiate(this.m_objectToSpawn, this.transform.position, this.transform.rotation);

            Rigidbody rb = this.m_object.GetComponent<Rigidbody>();
            if(rb != null)
            {
                rb.linearVelocity = m_initialVelocity;
            }

            // ah: reset flipped gravity on respawn
            GravityFlippedObject gravity_flip = this.m_object.GetComponent<GravityFlippedObject>();
            if(gravity_flip != null)
            {
                gravity_flip.ResetGravity();
            }
        }
    }

    public void Despawn()
    {
        if(this.m_object != null)
        {
            Destroy(this.m_object);
        }
    }

    void Update()
    {
        if(m_object == null && !m_spawnOnActivation)
        {
            Spawn();
        }
    }

}
