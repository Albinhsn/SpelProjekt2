using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [SerializeField]
    private Vector3 angles;

    [SerializeField]
    private float m_rotationSpeed;

    void FixedUpdate()
    {
        this.transform.Rotate(angles * m_rotationSpeed * Time.fixedDeltaTime);
    }
}
