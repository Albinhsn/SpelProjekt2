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
            this.m_object = Instantiate(this.m_objectToSpawn, this.transform.position, Quaternion.identity);;

            Rigidbody rb = this.m_object.GetComponent<Rigidbody>();
            if(rb != null)
            {
                rb.linearVelocity = m_initialVelocity;
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
