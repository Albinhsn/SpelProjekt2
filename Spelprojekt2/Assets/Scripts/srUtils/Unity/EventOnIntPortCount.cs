using System;
using srUtils.Unity.Events;
using srUtils.Unity.Ports;
using UnityEngine;

namespace srUtils.Unity
{
    [CreateAssetMenu (menuName = "Data/General/IntPortCountEventTrigger")] 
    public class EventOnIntPortCount : ScriptableObject
    {
        [SerializeField] private GlobalEventAsset eventAsset;
        [SerializeField] private IntPort port;
        [SerializeField] private int targetCount;

        private void OnEnable()//Build
        {
            if(port is not null) port.OnUpdated += CheckCount;
        }

        private void OnValidate()//Editor
        {
            if (port is not null)
            {
                port.OnUpdated -= CheckCount;
                port.OnUpdated += CheckCount;
            }
        }

        private void CheckCount(int value)
        {
            if (value == targetCount) eventAsset.Invoke();
        }
    }
}