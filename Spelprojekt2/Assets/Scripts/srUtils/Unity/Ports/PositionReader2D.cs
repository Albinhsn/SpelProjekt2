using System;
using UnityEngine;

namespace srUtils.Unity.Ports
{
    public class PositionReader2D : MonoBehaviour
    {
        [SerializeField] private Vector2Port port;

        private void Update()
        {
            port.value = transform.position;
        }
    }
}