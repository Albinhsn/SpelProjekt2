using System;
using UnityEngine;

namespace srUtils.Unity
{
    public class SetAnimatorInt : MonoBehaviour
    {
        [SerializeField] private string parameterName;
        [SerializeField] private int defaultValue;
        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            animator.SetInteger(parameterName, defaultValue);
        }

        public void SetValue(int value) => animator.SetInteger(parameterName, value);
    }
}