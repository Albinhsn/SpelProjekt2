using UnityEngine;

namespace SP2.Audio
{
    // Bra ersättning för "AudioSceneSettings"/"AudioTrigger" från äldre kurs.
    public sealed class AudioActionRunner : MonoBehaviour
    {
        [Header("== Action Sets ==")]
        [SerializeField] private AudioActionSetSO onStart;
        [SerializeField] private AudioActionSetSO onEnable;
        [SerializeField] private AudioActionSetSO onEnter;
        [SerializeField] private AudioActionSetSO onExit;

        [Header("== Trigger ==")]
        [SerializeField] private bool useTrigger = false;
        [SerializeField] private string requiredTag = "Player";

        [Header("== World pos for SFX ==")]
        [SerializeField] private bool overrideWorldPosForSfx = false;
        [SerializeField] private Transform sfxWorldPos;

        [Header("== Safety ==")]
        [Tooltip("Om objektet disableas utan exit: rensa global-param requests från denna runner.")]
        [SerializeField] private bool clearGlobalRequestsOnDisable = true;

        private Collider _col;

        private void Reset()
        {
            _col = GetComponent<Collider>();
            if (_col != null) _col.isTrigger = true;
        }

        private void Awake()
        {
            _col = GetComponent<Collider>();
            if (_col != null) _col.isTrigger = true;
        }

        private void Start()
        {
            Run(onStart);
        }

        private void OnEnable()
        {
            Run(onEnable);
        }

        private void OnDisable()
        {
            if (!clearGlobalRequestsOnDisable) return;

            // Rensa ALLA requests som denna runner kan ha satt
            ClearGlobalRequestsFromSet(onStart);
            ClearGlobalRequestsFromSet(onEnable);
            ClearGlobalRequestsFromSet(onEnter);
            ClearGlobalRequestsFromSet(onExit);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!useTrigger) return;
            if (!PassesFilter(other)) return;
            Run(onEnter);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!useTrigger) return;
            if (!PassesFilter(other)) return;
            Run(onExit);
        }

        private bool PassesFilter(Collider other)
        {
            if (string.IsNullOrEmpty(requiredTag)) return true;
            return other.CompareTag(requiredTag);
        }

        public void Run(AudioActionSetSO set)
        {
            if (set == null || set.actions == null || set.actions.Length == 0) return;

            var sys = AudioSystem.Instance;
            if (sys == null || sys.Config == null) return;

            Vector3 worldPos = transform.position;
            if (overrideWorldPosForSfx && sfxWorldPos != null)
                worldPos = sfxWorldPos.position;

            for (int i = 0; i < set.actions.Length; i++)
            {
                Execute(sys, set.actions[i], worldPos);
            }
        }

        private void Execute(AudioSystem sys, AudioAction a, Vector3 worldPos)
        {
            switch (a.type)
            {
                case AudioActionType.PlayCue:
                {
                    if (a.cue == null) return;
                    bool is2D = a.force2D || a.cue.is2D;
                    if (is2D) sys.Sfx?.Play2D(a.cue);
                    else sys.Sfx?.Play(a.cue, worldPos);
                    break;
                }

                case AudioActionType.SetMusicState:
                    sys.Music?.SetState(a.musicState);
                    break;

                case AudioActionType.SetMusicIntensity:
                    sys.Music?.SetIntensity(a.value01);
                    break;

                case AudioActionType.SetMusicDanger:
                    sys.Music?.SetDanger(a.value01);
                    break;

                case AudioActionType.SetGlobalParamRequest:
                {
                    string name = a.ResolveGlobalParamName(sys.Config);
                    if (string.IsNullOrEmpty(name)) return;
                    sys.Params?.SetRequest(this, name, Mathf.Clamp01(a.globalValue01), a.priority, a.fadeSeconds, a.useUnscaledTime);
                    break;
                }

                case AudioActionType.ClearGlobalParamRequest:
                {
                    string name = a.ResolveGlobalParamName(sys.Config);
                    if (string.IsNullOrEmpty(name)) return;
                    sys.Params?.ClearRequest(this, name);
                    break;
                }

                case AudioActionType.SetPauseSnapshot:
                    sys.Mixer?.SetPauseSnapshot(a.boolValue);
                    break;

                case AudioActionType.SetVolume:
                {
                    if (sys.Mixer == null) return;
                    float v = Mathf.Clamp01(a.volume01);

                    switch (a.volumeTarget)
                    {
                        case AudioVolumeTarget.Master: sys.Mixer.SetMaster(v); break;
                        case AudioVolumeTarget.Music: sys.Mixer.SetMusic(v); break;
                        case AudioVolumeTarget.Sfx: sys.Mixer.SetSfx(v); break;
                        case AudioVolumeTarget.Ui: sys.Mixer.SetUi(v); break;
                    }
                    break;
                }
            }
        }

        private void ClearGlobalRequestsFromSet(AudioActionSetSO set)
        {
            if (set == null || set.actions == null || set.actions.Length == 0) return;

            var sys = AudioSystem.Instance;
            if (sys == null || sys.Config == null || sys.Params == null) return;

            for (int i = 0; i < set.actions.Length; i++)
            {
                var a = set.actions[i];
                if (a.type != AudioActionType.SetGlobalParamRequest) continue;

                string name = a.ResolveGlobalParamName(sys.Config);
                if (string.IsNullOrEmpty(name)) continue;
                sys.Params.ClearRequest(this, name);
            }
        }
    }
}
