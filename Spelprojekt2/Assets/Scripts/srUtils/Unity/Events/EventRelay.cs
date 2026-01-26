using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace srUtils.Unity.Events
{
    public class EventRelay : MonoBehaviour
    {
        [SerializeField] [CanBeNull] private GlobalEventAsset eventAsset;
        [SerializeField] private UnityEvent localEvent;

        public void Invoke()
        {
            localEvent.Invoke();
        }

        private void Awake()
        {
            eventAsset?.Register(Invoke);
        }
        private void OnDisable()
        {
            eventAsset?.Unregister(Invoke);
        }


#if UNITY_EDITOR
        [SerializeField] private bool testInvoke;//Test event

        private void OnValidate()
        {
            if (testInvoke)
            {
                testInvoke = false;
                Invoke();
            }
        }
#endif
    }
}