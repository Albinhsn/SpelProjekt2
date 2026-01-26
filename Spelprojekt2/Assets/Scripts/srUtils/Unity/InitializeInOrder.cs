using System;
using UnityEngine;

namespace srUtils.Unity
{
    public class InitializeInOrder : MonoBehaviour
    {
        [SerializeField] private ResettableScriptableObject[] toInitialize;

        private void Start()
        {
            for (int a = 0; a < toInitialize.Length; a++)
            {
                toInitialize[a].Reset();
            }
            Destroy(this);
        }
    }
}