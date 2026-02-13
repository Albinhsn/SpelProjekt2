using System.Collections;
using UnityEngine;

// AudioKit anteckning
// Crossfade mellan två musik-event
// Själva mixen görs i FMOD, här bara parameter-fade

namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("AudioKit/Zoner/Layered Music Zone")]
    public sealed class LayeredMusicZone : MonoBehaviour
    {
        [Header("Event ID")]
        [SerializeField] private string layerAEventId = "Music_A";
        [SerializeField] private string layerBEventId = "Music_B";

        [Header("Parametrar")]
        [SerializeField] private AudioParamRef layerAParam;
        [SerializeField] private AudioParamRef layerBParam;

        [Header("Filter")]
        [SerializeField] private LayerMask activatorLayers = 0;
        [SerializeField] private string activatorTag = "Player";

        [Header("Gate")]
        [SerializeField] private bool startArmed = true;
        private bool armed;

        [Header("Fade")]
        [SerializeField] private float fadeSeconds = 0.6f;

        private Coroutine fadeRoutine;

        private void Reset()
        {
            var c = GetComponent<Collider>();
            if (c != null) c.isTrigger = true;
        }

        private void Awake()
        {
            armed = startArmed;
        }

        public void Arm(bool on) => armed = on;

        private void OnTriggerEnter(Collider other)
        {
            if (!armed) return;
            if (!IsActivator(other)) return;

            Crossfade(true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!armed) return;
            if (!IsActivator(other)) return;

            Crossfade(false);
        }

        private bool IsActivator(Collider other)
        {
            if (!string.IsNullOrEmpty(activatorTag) && !other.CompareTag(activatorTag))
                return false;

            if (activatorLayers != 0)
            {
                var mask = 1 << other.gameObject.layer;
                if ((activatorLayers.value & mask) == 0)
                    return false;
            }

            return true;
        }

        private void Crossfade(bool toLayerB)
        {
            if (AudioEventHub.I == null) return;

            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeRoutine(toLayerB));
        }

        private IEnumerator FadeRoutine(bool toLayerB)
        {
            var pA = layerAParam.Resolve(AudioResources.ParamLibrary);
            var pB = layerBParam.Resolve(AudioResources.ParamLibrary);
            if (string.IsNullOrEmpty(pA) || string.IsNullOrEmpty(pB)) yield break;

            AudioEventHub.I.PlayIfNotPlaying(layerAEventId);
            AudioEventHub.I.PlayIfNotPlaying(layerBEventId);

            if (!AudioEventHub.I.TryGet(layerAEventId, out var aInst)) yield break;
            if (!AudioEventHub.I.TryGet(layerBEventId, out var bInst)) yield break;

            float a0 = toLayerB ? 1f : 0f;
            float b0 = toLayerB ? 0f : 1f;

            float a1 = toLayerB ? 0f : 1f;
            float b1 = toLayerB ? 1f : 0f;

            float t = 0f;
            float d = Mathf.Max(0.02f, fadeSeconds);

            while (t < d)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / d);

                float aVal = Mathf.Lerp(a0, a1, u);
                float bVal = Mathf.Lerp(b0, b1, u);

                aInst.setParameterByName(pA, aVal);
                bInst.setParameterByName(pB, bVal);

                yield return null;
            }

            aInst.setParameterByName(pA, a1);
            bInst.setParameterByName(pB, b1);

            fadeRoutine = null;
        }
    }
}
