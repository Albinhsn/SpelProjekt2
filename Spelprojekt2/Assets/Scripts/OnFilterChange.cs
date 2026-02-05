using UnityEngine;
using UnityEngine.Events;
public class OnFilterChange : MonoBehaviour
{ 
    public FilterKind m_kind;
    [SerializeField]
    private UnityEvent onFilterActivated;
    [SerializeField]
    private UnityEvent onFilterDeactivated;
    
    public void Activate()
    {
        onFilterActivated?.Invoke();
    }

    public void Deactivate()
    {
        onFilterDeactivated?.Invoke();
    }
}

//// ah: activate objects
// OnFilterChange[] items = FindObjectsByType<OnFilterChange>(FindObjectsSortMode.None);
// for(int j = 0; j < items.Length; j++)
// {
//     if(items[j].m_kind == new_filter)
//     {
//         if(activating)
//         {
//             items[j].Activate();
//         }
//         else
//         {
//             items[j].Deactivate();
//         }
//     }
// }