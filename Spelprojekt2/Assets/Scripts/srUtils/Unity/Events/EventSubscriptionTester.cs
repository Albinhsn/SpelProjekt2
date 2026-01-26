using System;
using UnityEngine;

namespace srUtils.Unity.Events
{
    public class EventSubscriptionTester : MonoBehaviour
    {
        [SerializeField] private GlobalEventAsset[] events;
        [SerializeField] private bool check;

        private void Awake()
        {
            check = false;
            foreach (GlobalEventAsset obj in events)
            {
                if (obj.HasSubscribers())
                {
                    Debug.Log($"{obj.name} has subscribers", obj);
                }
            }
        }

        private void OnValidate()
        {
            if (check)
            {
                check = false;
                foreach (GlobalEventAsset obj in events)
                {
                    if (obj.HasSubscribers())
                    {
                        Debug.Log($"{obj.name} has subscribers", obj);
                    }
                }
            }
        }
    }
}