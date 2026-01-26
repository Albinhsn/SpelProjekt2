using System;
using JetBrains.Annotations;
using UnityEngine;

namespace srUtils.Unity.Ports
{
    [CreateAssetMenu(menuName = "Data/General/FloatPort")]
    public class FloatPort : ScriptableObject
    {
#if UNITY_EDITOR

        [SerializeField] private string description;
#endif

        [SerializeField] private float _value;
        [CanBeNull] public event Action<float> OnUpdated;

        public float value
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