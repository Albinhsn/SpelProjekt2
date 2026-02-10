using System;
using UnityEngine;

public class MovingPlatformReceiver : MonoBehaviour
{
    private Vector3 m_currentMovement = Vector3.zero;
    public event System.Action<Vector3> d_onMovementChanged;

    public Vector3 reference => m_currentMovement;

    public void UpdateReference(Vector3 reference)
    {
        d_onMovementChanged?.Invoke(reference);
        m_currentMovement = reference;
    }
}