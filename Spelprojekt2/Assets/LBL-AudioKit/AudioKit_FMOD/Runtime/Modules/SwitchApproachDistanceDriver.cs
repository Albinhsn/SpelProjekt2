using UnityEngine;

// AudioKit anteckning
// Sätter en parameter när spelaren är nära en target (threshold)
// Bra för att "toggle" en layer eller ett state utan att lerpa

namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    [AddComponentMenu("AudioKit/Drivers/Switch Approach Distance Driver")]
    public sealed class SwitchApproachDistanceDriver : MonoBehaviour
    {
        [Header("Parameter")]
        [SerializeField] private AudioParamRef parameter;

        [Header("Målpunkter")]
        [SerializeField] private Transform target;
        [SerializeField] private Transform player;
        [SerializeField] private string playerTag = "Player";

        [Header("Tröskel")]
        [SerializeField] private float distance = 10f;
        [SerializeField] private float valueNear = 1f;
        [SerializeField] private float valueFar = 0f;

        // Legacy (gömda)
        [SerializeField, HideInInspector] private string legacyParameterName = "Approach";

        private bool lastNear;

        private void Awake()
        {
            if (player == null)
            {
                var go = GameObject.FindGameObjectWithTag(playerTag);
                if (go != null) player = go.transform;
            }
        }

        private void Update()
        {
            if (AudioSystem.I == null) return;
            if (target == null || player == null) return;

            var name = ResolveParamName();
            if (string.IsNullOrEmpty(name)) return;

            var near = Vector3.Distance(player.position, target.position) <= distance;
            if (near == lastNear) return;
            lastNear = near;

            AudioSystem.I.SetGlobalParam(name, near ? valueNear : valueFar);
        }

        private string ResolveParamName()
        {
            var resolved = parameter.Resolve(AudioResources.ParamLibrary);
            if (!string.IsNullOrEmpty(resolved)) return resolved;
            return AudioParamUtil.Resolve(legacyParameterName);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (target == null) return;
            Gizmos.DrawWireSphere(target.position, distance);
        }
#endif
    }
}
