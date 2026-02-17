using UnityEngine;
using AudioKit.FMOD;

[RequireComponent(typeof(Collider))]
public class SoundOnCollision : MonoBehaviour
{

    [SerializeField] private AudioAction m_action;

    void OnCollisionEnter(Collision other)
    {
        m_action.Run(this.transform.position);
    }
}
