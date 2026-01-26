using System;
using System.Collections.Generic;
using UnityEngine;

namespace srUtils.Unity.Events
{
    [CreateAssetMenu(menuName = "Data/General/EventAsset")]
    public class EventAsset : ScriptableObject
    {
        private List<EventRelay> relays = new List<EventRelay>();

        public void Invoke()
        {
            for (int a = relays.Count; a >= 0; a--)
            {
                relays[a].Invoke();
            }
        }

        public void Register(EventRelay relay) => relays.Add(relay);
        public void Unregister(EventRelay relay) => relays.Remove(relay);
        
        
        
        
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