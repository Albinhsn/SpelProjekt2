using System;
using UnityEngine;

namespace srUtils.Unity
{
    public class ExistenceRegistrator : MonoBehaviour
    {
        [SerializeField] private InstanceSet register;

        private void OnEnable()
        {
            register.Add(gameObject);
        }

        private void OnDisable()
        {
            register.Remove(gameObject);
        }
    }
}