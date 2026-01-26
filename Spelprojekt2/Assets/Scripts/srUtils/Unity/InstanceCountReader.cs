using System;
using JetBrains.Annotations;
using srUtils.Unity.Events;
using srUtils.Unity.Ports;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

namespace srUtils.Unity
{
    [CreateAssetMenu (menuName = "Data/General/InstanceCountReader")]
    public class InstanceCountReader : ResettableScriptableObject
    {
        [SerializeField] private InstanceSet instanceSet;
        [CanBeNull] private InstanceSet _instanceSet;
        [SerializeField] private GlobalEventAsset eventAsset;
        [SerializeField] private IntPort targetCount;
        [SerializeField] private bool initialStateCheck = false;
        private bool hasUpdated;

        private void OnValidate()
        {
            if (instanceSet.IsUnityNull() || eventAsset.IsUnityNull())//If failure to validate, unregister and remove reference
            {
                Debug.LogError("instanceSet or eventAsset not set", this);
                if (_instanceSet is null) return;
                _instanceSet.OnUpdated -= CheckCount;
                _instanceSet = null;
                return;
            }
            
            if (_instanceSet != instanceSet)
            {
                if (_instanceSet is not null) _instanceSet.OnUpdated -= CheckCount;
                _instanceSet = instanceSet;
                _instanceSet.OnUpdated += CheckCount;
                Awake();
            }
        }

        private void Awake()
        {
            _instanceSet = instanceSet; //Will only be null if onValidate error was ignored (can this even happen?)
            _instanceSet.OnUpdated -= CheckCount;//Avoid duplicate registrations
            _instanceSet.OnUpdated += CheckCount;
            hasUpdated = false;
        }

        public override void Reset()
        {
            if(_instanceSet is not null) _instanceSet.OnUpdated -= CheckCount;//Avoid duplicate registrations
            _instanceSet = instanceSet; //Will only be null if onValidate error was ignored (can this even happen?)
            _instanceSet.OnUpdated -= CheckCount;//Avoid duplicate registrations
            _instanceSet.OnUpdated += CheckCount;
            hasUpdated = false;
        }

        private void CheckCount()
        {
            Assert.IsTrue(_instanceSet is not null, "internal instanceSet is null");
            if (!initialStateCheck && !hasUpdated)
            {
                if (_instanceSet.count == targetCount.value) return;
                hasUpdated = true;
            }
            if (_instanceSet.count == targetCount.value) eventAsset.Invoke();
        }
    }
}