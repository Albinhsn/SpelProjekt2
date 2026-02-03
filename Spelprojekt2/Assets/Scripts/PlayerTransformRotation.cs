using Unity.VisualScripting;
using UnityEngine;

public class PlayerTransformRotation : MonoBehaviour
{
    [SerializeField] float m_transformRotationSpeed = .02f;
    private Rigidbody m_rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if(m_rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(m_rb.linearVelocity.normalized, Vector3.up), m_transformRotationSpeed);
        }
    }
}
