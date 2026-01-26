using System;
using UnityEngine;

namespace srUtils.Unity
{
    public class RotateObjectToFaceDirection2D : MonoBehaviour
    {
        [SerializeField] private Transform target;

        public void Rotate(Vector2 direction)
        {
            target.rotation = Quaternion.AngleAxis(MathF.Atan2(direction.y, direction.x) * (MathF.PI / 180), Vector3.back);
        }
    }
}