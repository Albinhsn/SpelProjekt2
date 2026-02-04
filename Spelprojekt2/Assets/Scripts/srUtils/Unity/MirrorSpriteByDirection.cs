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
        private SpriteRenderer sprite_renderer;

        private void Awake()
        {
            sprite_renderer = GetComponent<SpriteRenderer>();
        }

        public void UpdateDirection(Vector2 direction)
        {
            if (mirrorX) sprite_renderer.flipX = direction.x > 0 ? invertX : !invertX;
            if (mirrorY) sprite_renderer.flipY = direction.y > 0 ? invertY : !invertY;
        }
    }
}
