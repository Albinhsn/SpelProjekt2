using System.Collections.Generic;
using UnityEngine;

// AudioKit anteckning
// Zon för musikparametrar
// Fade in och ut utan glitch
// Prio om zoner överlappar


namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class MusicParameterZone : MonoBehaviour
    {
        public enum ParameterMode { Known, Custom }

        [Header("Parameter")]
        [SerializeField] private ParameterMode mode = ParameterMode.Custom;
        [SerializeField] private KnownGlobalParam known = KnownGlobalParam.Indoor;
        [SerializeField] private string customParamName = "Indoor";

        [Header("Värden")]
        [SerializeField] private float enterValue = 1f;
        [SerializeField] private float exitValue;
        [SerializeField] private float fadeSeconds = 0.15f;

        [Header("Prioritet")]
        [SerializeField] private int priority;

        [Header("Target")]
        [SerializeField] private string requiredTag = "Player";
        [SerializeField] private bool applyIfStartsInside = true;
        [SerializeField] private bool resetOnDisable = true;
        [SerializeField] private bool forceTrigger = true;

        private int sourceId;
        private Collider zoneCollider;

        private readonly HashSet<Collider> inside = new HashSet<Collider>();

        private void Reset()
        {
            zoneCollider = GetComponent<Collider>();
            if (zoneCollider != null) zoneCollider.isTrigger = true;
        }

        private void OnValidate()
        {
            if (!forceTrigger) return;

            var c = GetComponent<Collider>();
            if (c != null) c.isTrigger = true;
        }

        private void Awake()
        {
            sourceId = GetInstanceID();
            zoneCollider = GetComponent<Collider>();

            if (forceTrigger && zoneCollider != null)
                zoneCollider.isTrigger = true;
        }

        private void Start()
        {
            if (!applyIfStartsInside) return;
            if (zoneCollider == null) return;
            if (string.IsNullOrEmpty(requiredTag)) return;

            var go = GameObject.FindGameObjectWithTag(requiredTag);
            if (go == null) return;

            var cols = go.GetComponentsInChildren<Collider>();
            if (cols == null || cols.Length == 0) return;

            for (int i = 0; i < cols.Length; i++)
            {
                var pc = cols[i];
                if (pc == null || !pc.enabled) continue;

                if (zoneCollider.bounds.Intersects(pc.bounds))
                {
                    inside.Add(pc);
                    Apply(true);
                    break;
                }
            }
        }

        private void OnDisable()
        {
            if (!resetOnDisable) return;

            inside.Clear();

            if (AudioParameterDriver.I == null) return;

            var name = ResolveName();
            if (string.IsNullOrEmpty(name)) return;

            AudioParameterDriver.I.SetSourceActive(name, sourceId, false, exitValue, priority, fadeSeconds);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (string.IsNullOrEmpty(requiredTag)) return;
            if (!other.CompareTag(requiredTag)) return;

            inside.Add(other);
            if (inside.Count == 1) Apply(true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (string.IsNullOrEmpty(requiredTag)) return;
            if (!other.CompareTag(requiredTag)) return;

            inside.Remove(other);
            if (inside.Count == 0) Apply(false);
        }

        private void Apply(bool entering)
        {
            if (AudioParameterDriver.I == null) return;

            var name = ResolveName();
            if (string.IsNullOrEmpty(name)) return;

            var v = entering ? enterValue : exitValue;
            AudioParameterDriver.I.SetSourceActive(name, sourceId, entering, v, priority, fadeSeconds);
        }

        private string ResolveName()
        {
            if (mode == ParameterMode.Custom) return customParamName;
            if (AudioParameterDriver.I == null) return null;
            return AudioParameterDriver.I.Resolve(known);
        }
    }
}
