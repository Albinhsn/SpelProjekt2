using UnityEngine;
using UnityEngine.Events;


public class Button : MonoBehaviour
{
    [SerializeField]
    private UnityEvent m_onClick;
    [SerializeField]
    private UnityEvent m_onRelease;

    private void OnTriggerEnter(Collider other)
    {
        // NOTE(ah): Compare this against something?
        m_onClick.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        // NOTE(ah): Compare this against something?
        m_onRelease.Invoke();
    }
}
