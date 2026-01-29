using UnityEngine;
using UnityEngine.UI;

namespace SP2.Audio
{
    public enum VolumeTarget
    {
        Master,
        Music,
        Sfx,
        Ui
    }

    [RequireComponent(typeof(Slider))]
    public sealed class AudioVolumeSlider : MonoBehaviour
    {
        [SerializeField] private VolumeTarget target = VolumeTarget.Master;
        [SerializeField] private Slider slider;

        private void Awake()
        {
            if (slider == null)
                slider = GetComponent<Slider>();

            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;

            // Init value
            var mixer = AudioSystem.Instance != null ? AudioSystem.Instance.Mixer : null;
            if (mixer != null)
            {
                slider.value = target switch
                {
                    VolumeTarget.Master => mixer.Master01,
                    VolumeTarget.Music => mixer.Music01,
                    VolumeTarget.Sfx => mixer.Sfx01,
                    VolumeTarget.Ui => mixer.Ui01,
                    _ => slider.value
                };
            }

            slider.onValueChanged.AddListener(OnChanged);
        }

        private void OnDestroy()
        {
            if (slider != null)
                slider.onValueChanged.RemoveListener(OnChanged);
        }

        private void OnChanged(float v)
        {
            var mixer = AudioSystem.Instance != null ? AudioSystem.Instance.Mixer : null;
            if (mixer == null) return;

            switch (target)
            {
                case VolumeTarget.Master: mixer.SetMaster(v); break;
                case VolumeTarget.Music: mixer.SetMusic(v); break;
                case VolumeTarget.Sfx: mixer.SetSfx(v); break;
                case VolumeTarget.Ui: mixer.SetUi(v); break;
            }
        }
    }
}
