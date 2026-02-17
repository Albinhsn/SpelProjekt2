using UnityEngine;

[RequireComponent(typeof(Collider))]
public class UnlockFilterOnTrigger : MonoBehaviour
{

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            FilterManager fm = FindFirstObjectByType<FilterManager>();
            fm.Unlock();
            Destroy(this.gameObject);
        }

    }
}
