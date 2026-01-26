using UnityEngine;

namespace srUtils.Unity
{
    public class SpriteReplacer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        public void SetSprite(Sprite sprite, Color color)
        {
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = color;
        }
        public void SetSprite(Sprite sprite)
        {
            SetSprite(sprite, Color.white);
        }
    }
}