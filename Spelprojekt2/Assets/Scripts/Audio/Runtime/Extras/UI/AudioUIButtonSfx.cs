using UnityEngine;
using UnityEngine.EventSystems;

namespace SP2.Audio
{
    // Lägg på samma GameObject som din Button (eller valfritt UI-element).
    public sealed class AudioUIButtonSfx : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
    {
        [Header("== Cues ==")]
        [SerializeField] private AudioCueSO click;
        [SerializeField] private AudioCueSO hover;

        [Header("== Options ==")]
        [SerializeField] private bool force2D = true;

        public void OnPointerClick(PointerEventData eventData)
        {
            var sys = AudioSystem.Instance;
            if (sys == null || click == null) return;

            if (force2D || click.is2D) sys.Sfx?.Play2D(click);
            else sys.Sfx?.Play(click, transform.position);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            var sys = AudioSystem.Instance;
            if (sys == null || hover == null) return;

            if (force2D || hover.is2D) sys.Sfx?.Play2D(hover);
            else sys.Sfx?.Play(hover, transform.position);
        }
    }
}
