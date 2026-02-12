using UnityEngine;

public class Fan : MonoBehaviour
{
    [Header("Lift Settings")]
    [SerializeField] float m_windStrength = 15f;
    [SerializeField] float m_maxHeight = 10f;

    [Header("Top Bob Settings")]
    [SerializeField] float m_bobAmplitude = 0.5f;
    [SerializeField] float m_bobSpeed = 2f;

    [Header("Centering Force")]
    [SerializeField] float m_XZcenteringStrength = 10f;

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;

        float height = DistanceToFanBase(other.transform);

        float topStartHeight = m_maxHeight - (m_bobAmplitude*2);

        if (height < topStartHeight)
        {
            float heightRatio = 1f - (height / m_maxHeight);
            float windForce = m_windStrength * heightRatio;

            rb.AddForce(transform.up * windForce, ForceMode.Acceleration);
        }
        else
        {
            float bobOffset = Mathf.Sin(Time.time * m_bobSpeed) * m_bobAmplitude;
            float targetY = transform.position.y + m_maxHeight + bobOffset;

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            Vector3 targetPosition = new Vector3(
                other.transform.position.x,
                targetY,
                other.transform.position.z
            );

            rb.MovePosition(Vector3.Lerp(
                other.transform.position,
                targetPosition,
                Time.deltaTime * 5f
            ));
        }



        Vector2 xzDistance = XZDistanceToCenter(other.transform);
        rb.AddForce(
            new Vector3(-xzDistance.x, 0, -xzDistance.y) * m_XZcenteringStrength,
            ForceMode.Acceleration
        );
    }

    float DistanceToFanBase(Transform playerTransform)
    {
        return playerTransform.position.y - transform.position.y;
    }

    Vector2 XZDistanceToCenter(Transform playerTransform)
    {
        return new Vector2(
            playerTransform.position.x - transform.position.x,
            playerTransform.position.z - transform.position.z
        );
    }
}
