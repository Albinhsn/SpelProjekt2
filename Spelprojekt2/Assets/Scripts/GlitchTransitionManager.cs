using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class GlitchTransitionManager : MonoBehaviour
{
    [SerializeField]
    public UnityEvent m_onTransitionEnd;

    [SerializeField]
    private Volume m_volume;

    private GlitchVolume m_glitchVolume;

    [SerializeField]
    private float m_transitionTime;

    private float m_transitionTimeRemaining;
    private bool m_transitioning;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float m_startIntensity;


    void Awake()
    {
        if(m_volume == null)
        {
            Debug.LogError("[GTM] Need a volume for transition");
        }
        else
        {
            m_volume.profile.TryGet(out m_glitchVolume);

            if(m_glitchVolume == null)
            {
                Debug.LogError("[GTM] Volume doesn't have a GlitchVolume");
            }
        }

    }

    void OnTriggerEnter(Collider other)
    {
        StartTransition();
    }

    public void RespawnPlayer()
    {
        LevelCheckpointManager.Respawn();
    }

    public void StartTransition()
    {
        m_transitionTimeRemaining = m_transitionTime;
        m_transitioning = true;
    }

    void EndTransition()
    {
        m_transitionTimeRemaining = 0;
        m_transitioning           = false;
        m_onTransitionEnd?.Invoke();
    }

    void Update()
    {

        if(m_transitioning)
        {
            m_transitionTimeRemaining -= Time.deltaTime;
            float t = Mathf.Lerp(m_startIntensity, 1.0f, 1.0f - m_transitionTimeRemaining / m_transitionTime);
            m_glitchVolume.m_intensity.value = t;
            if(m_transitionTimeRemaining <= 0)
            {
                EndTransition();
                m_glitchVolume.m_intensity.value = 0;
            }
        }
    }

}
