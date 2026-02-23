using UnityEngine;
using AudioKit.FMOD;
using FMODUnity;

public class BoxSoundOnCollision : MonoBehaviour
{
    [SerializeField] private AudioCueSO m_audioCue;

    [SerializeField]
    [Range(0.1f, 100.0f)]
    private float m_maxVelocity = 10.0f;

    private Rigidbody m_rb;

    void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision other)
    {
        if(other.impulse.sqrMagnitude > 0)
        {
            var si = RuntimeManager.CreateInstance(m_audioCue.evt);
            si.set3DAttributes(RuntimeUtils.To3DAttributes(this.transform.position));

            float ratio    = Mathf.Clamp(m_rb.linearVelocity.magnitude / m_maxVelocity, 0, 1);
            FMOD.RESULT ok = si.setParameterByName("velocity", ratio);
            ok = si.start();
            ok = si.release();
        }
    }
}
