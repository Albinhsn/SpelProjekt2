using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DynamicRBBarrier : MonoBehaviour
{

    void OnTriggerEnter(Collider other)
    {
        DynamicRigidbody d_rb = other.gameObject.GetComponent<DynamicRigidbody>();

        if(d_rb != null)
        {
            d_rb.Destroy();
        }

    }
}
