using UnityEngine;
using AudioKit.FMOD;
using FMODUnity;

public class BoxSoundOnCollision : MonoBehaviour
{
    [SerializeField] private AudioCueSO m_audioCue;

    [SerializeField]
    [Range(0.1f, 100.0f)]
    private float m_maxVelocity = 10.0f;

    private FMOD.Studio.EventInstance m_soundInstance;
    private Rigidbody m_rb;

    void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
    }

    void OnDestroy()
    {
    }

    void OnCollisionEnter(Collision other)
    {
        if(other.impulse.sqrMagnitude > 0)
        {
            Debug.Log("aa");
            m_soundInstance = RuntimeManager.CreateInstance(m_audioCue.evt);
            m_soundInstance.set3DAttributes(RuntimeUtils.To3DAttributes(this.transform.position));

            float ratio    = Mathf.Clamp(m_rb.linearVelocity.magnitude / m_maxVelocity, 0, 1);
            FMOD.RESULT ok = m_soundInstance.setParameterByName("velocity", ratio);
            ok = m_soundInstance.start();
            ok = m_soundInstance.release();
        }
    }
}
