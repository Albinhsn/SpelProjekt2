using UnityEngine;

public class SendPickupRequest : MonoBehaviour
{

    void Update()
    {
        foreach(var rb in FindObjectsByType<DynamicRigidbody>(FindObjectsSortMode.None))
        {
            PickupItemIndicatorManager.Request(this.transform.position, this.transform.forward, this.transform.up, rb.transform.position);
        }
    }
}
