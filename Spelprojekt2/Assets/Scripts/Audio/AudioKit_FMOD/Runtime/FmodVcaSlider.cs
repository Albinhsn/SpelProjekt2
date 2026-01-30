using UnityEngine;
using UnityEngine.UI;
using FMOD.Studio;
using FMODUnity;

// AudioKit anteckning
// UI slider kopplad till en VCA
// Bra för options-menyn
// Kan spara värde i PlayerPrefs


namespace AudioKit.FMOD
{
    [RequireComponent(typeof(Slider))]
    public sealed class FmodVcaSlider : MonoBehaviour
    {
        [SerializeField] private string vcaPath = "vca:/Master";
        [SerializeField] private Slider slider;

        private VCA vca;

        private void Awake()
        {
            if (slider == null) slider = GetComponent<Slider>();
            if (slider == null) return;

            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;

            vca = RuntimeManager.GetVCA(vcaPath);
            if (!vca.isValid()) return;

            vca.getVolume(out var current, out _);
            slider.SetValueWithoutNotify(current);

            slider.onValueChanged.AddListener(OnChanged);
        }

        private void OnDestroy()
        {
            if (slider != null)
                slider.onValueChanged.RemoveListener(OnChanged);
        }

        private void OnChanged(float v)
        {
            if (!vca.isValid()) return;
            vca.setVolume(Mathf.Clamp01(v));
        }
    }
}
