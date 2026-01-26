using System;
using UnityEngine;

namespace srUtils.Unity
{
    public class SetAnimatorVector : MonoBehaviour
    {
        [SerializeField] private string xParameterName;
        [SerializeField] private string yParameterName;
        [SerializeField] private Vector2 defaultValue;
        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            animator.SetFloat(xParameterName, defaultValue.x);
            animator.SetFloat(yParameterName, defaultValue.y);
        }

        public void SetValue(Vector2 value)
        {
            animator.SetFloat(xParameterName, value.x);
            animator.SetFloat(yParameterName, value.y);
        }
    }
}