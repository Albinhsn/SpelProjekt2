using UnityEngine;
using AudioKit.FMOD;

[RequireComponent(typeof(Collider))]
public class SetFMODParameterOnTrigger : MonoBehaviour
{

    [SerializeField]
    private string m_event;

    [SerializeField]
    private string m_parameter;

    [SerializeField]
    private float m_onEnter;

    [SerializeField]
    private float m_onExit;


    void OnTriggerEnter()
    {
        var hub = AudioEventHub.I;
        if(hub != null) hub.SetEventParam(m_event, m_parameter, m_onEnter);
    }

    void OnTriggerExit()
    {
        var hub = AudioEventHub.I;
        if(hub != null) hub.SetEventParam(m_event, m_parameter, m_onExit);
    }
}
