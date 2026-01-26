using System;
using JetBrains.Annotations;
using UnityEngine;

namespace srUtils.Unity.Ports
{
    [CreateAssetMenu(menuName = "Data/General/IntegerPort")]
    public class IntPort : ScriptableObject
    {
#if UNITY_EDITOR

        [SerializeField] private string description;
#endif

        [SerializeField] private int _value;
        [CanBeNull] public event Action<int> OnUpdated;

        public int value
        {
            get => _value;
            set
            {
                _value = value;
                OnUpdated?.Invoke(_value);
            }
        }
    }
}