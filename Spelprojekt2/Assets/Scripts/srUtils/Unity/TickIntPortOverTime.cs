using System;
using System.Collections;
using JetBrains.Annotations;
using srUtils.Unity.Events;
using srUtils.Unity.Ports;
using Unity.VisualScripting;
using UnityEngine;

namespace srUtils.Unity
{
    public class TickIntPortOverTime : MonoBehaviour
    {
        [SerializeField] private IntPort port;
        [SerializeField] private float timeBetweenTicks;
        [SerializeField] private int step;
        private bool running;

        private void Awake()
        {
            timeRemaining = timeBetweenTicks;
            running = true;
        }

        private float timeRemaining;

        private void Update()
        {
            if (!running) return;
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                if (timeRemaining <= 0)
                {
                    timeRemaining = timeBetweenTicks;
                    port.value += step;
                }
            }

        }
    }
}