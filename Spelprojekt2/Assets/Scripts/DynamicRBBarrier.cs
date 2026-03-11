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

        if(other.CompareTag("Player"))
        {
            FilterManager fm = FindFirstObjectByType<FilterManager>();
            fm.ChangeFilter(FilterKind.None);
        }

    }
}
