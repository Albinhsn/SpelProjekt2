using System;
using UnityEngine;

namespace srUtils.Unity
{
    public class MirrorSpriteByDirection : MonoBehaviour
    {
        [SerializeField] private bool mirrorX;
        [SerializeField] private bool mirrorY;
        [SerializeField] private bool invertX;
        [SerializeField] private bool invertY;
        private SpriteRenderer renderer;

        private void Awake()
        {
            renderer = GetComponent<SpriteRenderer>();
        }

        public void UpdateDirection(Vector2 direction)
        {
            if (mirrorX) renderer.flipX = direction.x > 0 ? invertX : !invertX;
            if (mirrorY) renderer.flipY = direction.y > 0 ? invertY : !invertY;
        }
    }
}