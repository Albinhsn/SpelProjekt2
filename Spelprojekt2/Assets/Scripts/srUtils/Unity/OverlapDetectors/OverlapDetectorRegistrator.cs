using System;
using UnityEngine;

namespace srUtils.Unity
{
    public class OverlapDetectorRegistrator : MonoBehaviour
    {
        [SerializeField] private InstanceSet instanceSet;

        public void DeactivateImmediate(OverlapDetector detector)
        {
            instanceSet.Remove(detector);
        }
        private void OnEnable()
        {
            instanceSet.Add(GetComponent<OverlapDetector>());
        }
        private void OnDisable()
        {
            instanceSet.Remove(GetComponent<OverlapDetector>());
        }
    }
}