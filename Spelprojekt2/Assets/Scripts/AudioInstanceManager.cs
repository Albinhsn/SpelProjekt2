using UnityEngine;
using AudioKit.FMOD;
using FMODUnity;
using FMOD.Studio;

public class AudioInstanceManager : MonoBehaviour
{
    [SerializeField]
    private string[] m_eventIDs;

    [SerializeField]
    private AudioCueSO cue;


    void Awake()
    {
#if false
        var si = RuntimeManager.CreateInstance(cue.evt);

        EventDescription event_desc;
        si.getDescription(out event_desc);

        event_desc.getParameterDescriptionCount(out int count);

        for(int i = 0; i < count; i++)
        {
            PARAMETER_DESCRIPTION desc;
            event_desc.getParameterDescriptionByIndex(i, out desc);
            Debug.Log($"Param: {(string)desc.name}");
        }
#endif


        if(m_eventIDs != null && m_eventIDs.Length > 0)
        {
            var hub = AudioEventHub.I;
            for(int i = 0; i < m_eventIDs.Length; i++)
            {
                hub?.StartLoop(m_eventIDs[i]);
            }
        }
    }

    void OnDisable()
    {
        if(m_eventIDs != null && m_eventIDs.Length > 0)
        {
            var hub = AudioEventHub.I;
            for(int i = 0; i < m_eventIDs.Length; i++)
            {
                hub?.StopLoop(m_eventIDs[i]);
            }
        }
    }

    void OnEnable()
    {
        Awake();
    }

    void OnDestroy()
    {
        OnDisable();
    }
}
