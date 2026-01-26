using System;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace srUtils.Unity.Events
{
    public class EventSequence : MonoBehaviour
    {
        [SerializeField] [CanBeNull] private GlobalEventAsset triggerEvent;
        [SerializeField] public Entry[] sequence;
        [SerializeField] private bool repeat;
        private int index;
        private float delay;
        private bool active;

        private void Awake()
        {
            if (triggerEvent is null)
            {
                Reset();
            }
            else
            {
                triggerEvent.Register(Reset);
            }
        }

        private void OnDisable()
        {
            triggerEvent?.Unregister(Reset);
        }

        private void FixedUpdate()
        {
            if (!active) return;
            if (delay > 0)
            {
                delay -= Time.fixedDeltaTime;
                return;
            }
            
            sequence[index].eventAsset?.Invoke();
            sequence[index].localEvent.Invoke();
            index++;
            if (index >= sequence.Length)
            {
                if (repeat) index = 0;
                else
                {
                    if(triggerEvent is null) Destroy(this);
                    active = false;
                    return;
                }
            }
            delay = sequence[index].delay;
        }

        public void Reset()
        {
            delay = sequence[0].delay;
            index = 0;
            active = true;
        }

        [System.Serializable]
        public class Entry
        {
            public float delay;
            [CanBeNull] public GlobalEventAsset eventAsset;
            public UnityEvent localEvent;
        }
    }
}