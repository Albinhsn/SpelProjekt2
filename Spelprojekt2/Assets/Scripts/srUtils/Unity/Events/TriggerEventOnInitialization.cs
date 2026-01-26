using UnityEngine;

namespace srUtils.Unity.Events
{
    [CreateAssetMenu(menuName = "Data/General/InitializationEventTrigger")]
    public class TriggerEventOnInitialization : ResettableScriptableObject
    {
        [SerializeField] private GlobalEventAsset Event;
        public override void Reset()
        {
            Event.Invoke();
        }
    }
}