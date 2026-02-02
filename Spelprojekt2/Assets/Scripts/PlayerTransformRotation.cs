using UnityEngine;

public class PlayerTransformRotation : MonoBehaviour
{
    private Rigidbody m_rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(m_rb.linearVelocity.normalized, Vector3.up);
    }
}
