using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using System.Collections;

[RequireComponent(typeof(Volume))]
public class GlitchTransitionManager : MonoBehaviour
{

    private static GlitchTransitionManager I;

    // Volume data
    private Volume m_volume;
    private GlitchVolume m_glitchVolume;

    // Events
    private UnityEvent m_onTransitionEnd_;
    public static UnityEvent m_onTransitionEnd => I.m_onTransitionEnd_;

    // Transition settings
    private float m_transitionTime;
    private float m_transitionTimeRemaining;
    private bool m_transitioning;
    private float m_startIntensity;


    void Awake()
    {
        if(I != null && I != this)
        {
            Destroy(this.gameObject);
            return;
        }

        I = this;
        m_onTransitionEnd_ = new();
        m_volume = GetComponent<Volume>();

        if(m_volume == null)
        {
            Debug.LogError("[GTM] Need a volume for transition");
        }
        else if(m_glitchVolume == null)
        {
            m_volume.profile.TryGet(out m_glitchVolume);

            if(m_glitchVolume == null)
            {
                Debug.LogError("[GTM] Volume doesn't have a GlitchVolume");
            }
        }

    }

    public static bool StartTransition(float time, float start_intensity)
    {
        bool result = false;
        if(!I.m_transitioning)
        {
            I.m_transitioning           = true;
            I.m_transitionTime          = time;
            I.m_transitionTimeRemaining = time;
            I.m_startIntensity          = start_intensity;

            // ah: Take framebuffer snapshot
            {
                FramebufferSnapshotManager.Request();
                FramebufferDisplaySnapshotPass.Activate();
            }

            result = true;

            I.StartCoroutine(UpdateTransition());
        }
        return result;
    }

    private static void EndTransition()
    {
        I.m_transitionTimeRemaining = 0;
        I.m_transitioning           = false;
        I.m_onTransitionEnd_?.Invoke();
        FramebufferDisplaySnapshotPass.Deactivate();
    }

    static IEnumerator UpdateTransition()
    {
        for(;;)
        {
            I.m_transitionTimeRemaining -= Time.deltaTime;
            float t = Mathf.Lerp(I.m_startIntensity, 1.0f, 1.0f - I.m_transitionTimeRemaining / I.m_transitionTime);
            I.m_glitchVolume.m_intensity.value = t;

            if(I.m_transitionTimeRemaining > 0)
            {
                yield return null;
            }
            else
            {
                break;
            }

        }

        EndTransition();
        I.m_glitchVolume.m_intensity.value = 0;
    }

}
