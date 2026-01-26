using UnityEngine;

namespace srUtils.Unity.Ports
{
    public class IntPortAdder : MonoBehaviour
    {
        [SerializeField] private IntPort port;
        [SerializeField] private int savedValue;
        
        public void SetValue(int value)
        {
            savedValue = value;
        }

        public void AddSavedValue()
        {
            port.value += savedValue;
        }

        public void Add(int value)
        {
            port.value += value;
        }
    }
}