using System;
using JetBrains.Annotations;
using UnityEngine;

namespace srUtils.Unity.Ports
{
    [CreateAssetMenu(menuName = "Data/General/2DVectorPort")]
    public class Vector2Port : ScriptableObject
    {
#if UNITY_EDITOR

        [SerializeField] private string description;
#endif

        [SerializeField] private Vector2 _value;
        [CanBeNull] public event Action<Vector2> OnUpdated;

        public Vector2 value
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