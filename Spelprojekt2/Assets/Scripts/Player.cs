using UnityEngine;
using UnityEngine.Events;
using static LinAlg.LinAlg;
using UnityEngine.InputSystem;
using AudioKit.FMOD;
using Unity.VisualScripting;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class Player : MonoBehaviour
{

    [Header("Filter events")]
    [SerializeField]
    public UnityEvent<FilterKind> m_onTriggerEnterWithFilter;

    [SerializeField]
    public UnityEvent<FilterKind> m_onTriggerLeaveWithFilter;


    void Start()
    {
        PickupItemIndicatorManager.SetSourceTransform(this.transform);
        FilterManager fm = FindFirstObjectByType<FilterManager>();
        this.m_onTriggerEnterWithFilter.AddListener(fm.SetCollisionEnterWithFilter);
        this.m_onTriggerLeaveWithFilter.AddListener(fm.SetCollisionLeaveWithFilter);
    }

    void OnTriggerEnter(Collider other)
    {
        int filter_value = FilterObject.TagIsFilter(other.gameObject.tag);
        if(filter_value != -1)
        {
            m_onTriggerEnterWithFilter?.Invoke((FilterKind)filter_value);
        }
    }

    void OnTriggerExit(Collider other)
    {
        int filter_value = FilterObject.TagIsFilter(other.gameObject.tag);
        if(filter_value != -1)
        {
            m_onTriggerLeaveWithFilter?.Invoke((FilterKind)filter_value);
        }
    }

}
