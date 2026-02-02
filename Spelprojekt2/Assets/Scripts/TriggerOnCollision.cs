using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class TriggerOnCollision : MonoBehaviour
{

    [SerializeField]
    private UnityEvent onTrigger;

    void OnTriggerEnter(Collider other)
    {
        onTrigger?.Invoke();
    }


}
