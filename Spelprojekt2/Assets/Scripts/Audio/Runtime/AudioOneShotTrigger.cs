using UnityEngine;

namespace SP2.Audio
{
    [RequireComponent(typeof(Collider))]
    public sealed class AudioOneShotTrigger : MonoBehaviour
    {
        [SerializeField] private AudioCueSO cue;
        [SerializeField] private bool play2D = false;
        [SerializeField] private bool onlyOnce = true;
        [SerializeField] private string requiredTag = "Player";

        private Collider _col;
        private bool _used;

        private void Reset()
        {
            _col = GetComponent<Collider>();
            if (_col != null) _col.isTrigger = true;
        }

        private void Awake()
        {
            _col = GetComponent<Collider>();
            _col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (onlyOnce && _used) return;
            if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

            var sys = AudioSystem.Instance;
            if (sys == null) return;

            if (play2D)
                sys.Sfx?.Play2D(cue);
            else
                sys.Sfx?.Play(cue, transform.position);

            _used = true;
        }
    }
}
