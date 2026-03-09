using UnityEngine;
using AudioKit.FMOD;
using FMODUnity;

public class Fan : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioCueSO m_soundCue;

    private FMOD.Studio.EventInstance m_soundInstance;

    [Header("Lift Settings")]
    [SerializeField] float m_windStrength = 15f;
    [SerializeField] float m_maxHeight = 10f;
    [SerializeField] float m_xzPullStrength = 10f;

    [Header("Top Bob Settings")]
    [SerializeField] float m_bobAmplitude = 0.5f;
    [SerializeField] float m_bobSpeed = 2f;

    [SerializeField] private ParticleSystem m_particleSystem;

    [Header("Start active")]
    [SerializeField]
    private bool m_isActive;

    private float m_colliderHeight;
    private CapsuleCollider m_collider;
    private bool m_soundIsPlaying;

    void Start()
    {
        m_soundInstance = RuntimeManager.CreateInstance(m_soundCue.evt);
        m_soundInstance.set3DAttributes(RuntimeUtils.To3DAttributes(this.transform.position));
        m_soundInstance.start();

        if(m_isActive)
        {
            TurnOn();
        }
    }

    void OnDestroy()
    {
        m_soundInstance.setParameterByName("looping", 0.0f);
        m_soundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        m_soundInstance.release();
    }

    public void TurnOff()
    {
        m_isActive = false;
        StopSound();
        if(m_particleSystem != null)
        {
            m_particleSystem.Stop();
        }
    }

    public void TurnOn()
    {
        m_isActive = true;
        bool should_play_sound = true;
        FilterObject filter = GetComponentInParent<FilterObject>();
        if(filter != null)
        {
            should_play_sound = filter.m_kind != FilterManager.m_activeFilter;
        }
        if(m_particleSystem != null)
        {
            m_particleSystem.Play();
        }

        if(should_play_sound)
        {
            PlaySound();
        }
    }



    float timer = 0f;
    void OnValidate()
    {
        m_collider = GetComponent<CapsuleCollider>();
        m_colliderHeight = m_maxHeight + 5f;
        m_collider.height = m_colliderHeight;
        m_collider.center = new Vector3(0f, m_colliderHeight / 2f, 0f);
    }

    void OnTriggerStay(Collider other)
    {
        if(m_isActive)
        {
            if (!other.CompareTag("Player") && other.gameObject.GetComponent<DynamicRigidbody>() == null) return;

            if(other.gameObject.GetComponent<DynamicRigidbody>() != null)
            {
                if (other.gameObject.GetComponent<Rigidbody>().isKinematic) return;
            }

            if(gameObject.GetComponentInParent<FilterObject>() != null && gameObject.GetComponentInParent<FilterObject>().Activated) return;

            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb == null) return;

            float height = DistanceToFanBase(other.transform);

            float topStartHeight = m_maxHeight - (m_bobAmplitude*2);

            timer += Time.deltaTime;

            if (height < topStartHeight)
            {
                float targetY = other.transform.position.y + m_windStrength * .1f;
                rb.MovePosition(new Vector3(
                    other.transform.position.x,
                    targetY,
                    other.transform.position.z
                ));

                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                timer = 0f;
            }
            else
            {
                float bobOffset = Mathf.Sin(timer * m_bobSpeed) * m_bobAmplitude;
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
            rb.linearVelocity = new Vector3(
                -xzDistance.x * m_xzPullStrength,
                rb.linearVelocity.y,
                -xzDistance.y * m_xzPullStrength
            );
        }
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

    void PlaySound()
    {
        m_soundIsPlaying = true;
        m_soundInstance.setParameterByName("looping", 1.0f);
    }

    void StopSound()
    {
        m_soundInstance.setParameterByName("looping", 0.0f);
    }

    void Update()
    {
        if(m_isActive)
        {
            bool should_play_sound = true;
            FilterObject filter = GetComponentInParent<FilterObject>();
            if(filter != null)
            {
                should_play_sound = filter.m_kind != FilterManager.m_activeFilter;
            }

            if(should_play_sound && !m_soundIsPlaying)
            {
                PlaySound();
            }

            if(!should_play_sound && m_soundIsPlaying)
            {
                StopSound();
            }
        }

    }
}
