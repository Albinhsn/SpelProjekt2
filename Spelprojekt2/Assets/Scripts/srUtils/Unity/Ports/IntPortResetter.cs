using System;
using UnityEngine;

namespace srUtils.Unity.Ports
{
    public class IntPortResetter : MonoBehaviour
    {
        [SerializeField] private IntPort port;
        [SerializeField] private int value;

        private void Awake()
        {
            port.value = value;
            Destroy(this);
        }
    }
}