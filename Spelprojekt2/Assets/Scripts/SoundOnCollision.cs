using UnityEngine;
using AudioKit.FMOD;

[RequireComponent(typeof(Collider))]
public class SoundOnCollision : MonoBehaviour
{

    [SerializeField] private AudioAction m_action;

    void OnCollisionEnter(Collision other)
    {
        if(other.impulse.sqrMagnitude > 0)
        {
            m_action.Run(this.transform.position);
        }
    }
}
