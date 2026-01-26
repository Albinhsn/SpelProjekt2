using UnityEngine;
using UnityEngine.Events;

namespace srUtils.Unity
{
    public class LocalCounter : MonoBehaviour
    {
        
        [SerializeField] private int _target;
        public int target
        {
            get => _target;
            set
            {
                _target = value;
                CheckMatch();
            }
        }

        [SerializeField] private int _value;
        public int value
        {
            get => _value;
            set
            {
                _value = value;
                onValueUpdated.Invoke(_value);
                CheckMatch();
            } 
        }

        [SerializeField] private UnityEvent onTargetReached;
        [SerializeField] private UnityEvent<int> onValueUpdated;

        private void CheckMatch()
        {
            if (!isActiveAndEnabled) return;
            if (_value == _target) onTargetReached.Invoke();
        }

        public void Add(int value) => this.value += value;
        public void Subtract(int value) => this.value -= value;
    }
}