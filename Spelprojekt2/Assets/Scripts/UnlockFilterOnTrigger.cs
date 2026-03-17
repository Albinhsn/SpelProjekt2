using UnityEngine;

[RequireComponent(typeof(Collider))]
public class UnlockFilterOnTrigger : MonoBehaviour
{

    public void Unlock()
    {
        FilterManager fm = FindFirstObjectByType<FilterManager>();
        fm.Unlock();
        Destroy(this.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            Unlock();
        }

    }
}
