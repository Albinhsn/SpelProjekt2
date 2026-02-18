using UnityEngine;
using AudioKit.FMOD;

public class SetCutsceneMusicParameter : MonoBehaviour
{
    [SerializeField] private string m_parameter;
    [SerializeField] private string m_eventID;

    public void SetEventParameter()
    {
        if(!string.IsNullOrEmpty(m_eventID) && !string.IsNullOrEmpty(m_parameter))
        {
            var hub = AudioEventHub.I;
            if(hub != null) hub.SetEventParam(m_eventID, m_parameter, 1.0f);
        }

        Destroy(this.gameObject);
    }
}
