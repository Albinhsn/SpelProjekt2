using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyStartAsleep : MonoBehaviour
{
    void Awake()
    {
        var rb = GetComponent<Rigidbody>();
        rb.Sleep();
    }
}
