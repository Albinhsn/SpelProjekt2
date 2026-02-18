using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class DestroyObjectOnParticleCompletion : MonoBehaviour
{

    private ParticleSystem ps;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {

        if(ps.isStopped)
        {
            Destroy(this.gameObject);
        }
        
    }
}
