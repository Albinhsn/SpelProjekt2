using System;
using JetBrains.Annotations;
using UnityEngine;

namespace srUtils.Unity.Events
{
    public class InstantEventSequence : MonoBehaviour
    {
        [SerializeField] [CanBeNull] private GlobalEventAsset triggerEvent;
        [SerializeField] private GlobalEventAsset[] events;

        private void Awake()
        {
            triggerEvent?.Register(Trigger);
        }
        private void OnDisable()
        {
            triggerEvent?.Unregister(Trigger);
        }

        public void Trigger()
        {
            for (int a = 0; a < events.Length; a++)
            {
                events[a].Invoke();
            }
        }
    }
}