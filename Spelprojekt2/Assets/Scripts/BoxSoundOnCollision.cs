using UnityEngine;
using AudioKit.FMOD;
using FMODUnity;

public class BoxSoundOnCollision : MonoBehaviour
{
    [SerializeField] private AudioCueSO m_audioCue;

    private Rigidbody m_rb;
    private BoxSoundVelocity m_velocity;

    void Awake()
    {
        m_rb       = GetComponent<Rigidbody>();
        m_velocity = Resources.Load<BoxSoundVelocity>("Audio/BoxSoundVelocity") as BoxSoundVelocity;
    }

    void OnCollisionEnter(Collision other)
    {
        if(other.impulse.sqrMagnitude > 0)
        {
            var si = RuntimeManager.CreateInstance(m_audioCue.evt);
            si.set3DAttributes(RuntimeUtils.To3DAttributes(this.transform.position));

            float ratio    = Mathf.Clamp(m_rb.linearVelocity.magnitude / m_velocity.m_velocity, 0, 1);
            FMOD.RESULT ok = si.setParameterByName("velocity", ratio);
            ok = si.start();
            ok = si.release();
        }
    }
}
