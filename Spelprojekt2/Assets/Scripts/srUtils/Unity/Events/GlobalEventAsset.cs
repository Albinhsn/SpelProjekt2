using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace srUtils.Unity.Events
{
    [CreateAssetMenu (menuName = "Data/General/GlobalEventAsset")]
    public class GlobalEventAsset : ScriptableObject
    {
        private event Action Event;
        public void Register(Action action)
        {
            Event += action;
        }
        public void Unregister(Action action)
        {
            Event -= action;
        }

        public void Invoke()
        {
            Event?.Invoke();
        }

        public bool HasSubscribers() => Event is not null;
        
#if UNITY_EDITOR
        [SerializeField] private bool testInvoke;//Test event
        [SerializeField] private bool purge;

        private void OnValidate()
        {
            if (purge)
            {
                purge = false;
                Event = null;
            }
            if (testInvoke)
            {
                testInvoke = false;
                Invoke();
            }
        }
#endif
    }
}