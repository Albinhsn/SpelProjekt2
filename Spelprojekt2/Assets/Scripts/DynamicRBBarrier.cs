using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class DynamicRBBarrier : MonoBehaviour
{

    [SerializeField]
    private UnityEvent m_onDynamicRigidbodyGotDestroyed;

    void OnTriggerEnter(Collider other)
    {
        DynamicRigidbody d_rb = other.gameObject.GetComponent<DynamicRigidbody>();

        if(d_rb != null)
        {
            m_onDynamicRigidbodyGotDestroyed?.Invoke();
            d_rb.Destroy();
        }

        if(other.CompareTag("Player"))
        {
            FilterManager fm = FindFirstObjectByType<FilterManager>();
            fm.ChangeFilter(FilterKind.None);
        }

    }
}
