using System;
using UnityEngine;
using UnityEngine.Events;

namespace srUtils.Unity
{
    public class EventOnAwake : MonoBehaviour
    {
        [SerializeField] private UnityEvent onAwake;

        private void Awake()
        {
            onAwake.Invoke();
            Destroy(this);
        }
    }
}