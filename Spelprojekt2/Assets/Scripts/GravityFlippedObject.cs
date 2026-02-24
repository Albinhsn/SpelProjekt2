using UnityEngine;
using System;


public enum GravityDirection
{
    Up,
    Down,
}


[RequireComponent(typeof(Rigidbody))]
public class GravityFlippedObject : MonoBehaviour
{
    public Guid m_ID;

    [SerializeField]
    private GravityDirection m_startingState;

    Vector3 m_flippedGravity = new Vector3(0, 9.8f, 0);
    Rigidbody m_rb;

    public bool m_useGravity => m_rb.useGravity;

    void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        Awake();
        ResetGravity();
    }

    public void SetGravity(bool use_gravity)
    {
        m_rb.useGravity = use_gravity;
    }

    public void ResetGravity()
    {
        m_rb.useGravity = m_startingState == GravityDirection.Up;
    }


    public void Flip()
    {
        // ah: flip gravity
        m_rb.useGravity = !m_rb.useGravity;

        // ah: reset the vertical velocity when we flip
        // since the gravity is acceleration based 
        // so we avoid some bobbing
        var v = m_rb.linearVelocity;
        m_rb.linearVelocity = new(v.x, 0, v.z);
    }

    void FixedUpdate()
    {
        // NOTE(ah): prefer unity be the one to manage gravity, but yeah
        // this is the only way i found to do this
        if(!m_rb.useGravity)
        {
            m_rb.AddForce(m_flippedGravity, ForceMode.Acceleration);
        }
    }
}
