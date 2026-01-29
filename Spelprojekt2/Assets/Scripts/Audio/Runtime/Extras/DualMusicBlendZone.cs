using System.Collections;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

namespace SP2.Audio
{
    // Legacy-style boss blend: main music + separate boss music, crossfaded via local params.

    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    public sealed class DualMusicBlendZone : MonoBehaviour
    {
        [Header("== Events ==")]
        [SerializeField] private bool useMainFromConfig = true;
        [SerializeField] private EventReference mainMusic;
        [SerializeField] private EventReference bossMusic;

        [Header("== Local params (0..1) ==")]
        [SerializeField] private string mainAudibleParam = "MainAudible";
        [SerializeField] private string bossAudibleParam = "BossAudible";

        [Header("== Activator ==")]
        [SerializeField] private LayerMask activatorLayers = 0;

        [Header("== Gate ==")]
        [SerializeField] private bool startArmed = false;

        [Header("== Fade ==")]
        [Min(0f)] [SerializeField] private float armFadeSeconds = 0.5f;
        [Min(0f)] [SerializeField] private float zoneFadeSeconds = 0.2f;
        [Min(0f)] [SerializeField] private float exitGraceSeconds = 0.15f;

        private BoxCollider _box;
        private bool _armed;
        private bool _inside;
        private float _lastInside;

        private float _curMain = 1f;
        private float _curBoss = 0f;
        private float _tMain = 1f;
        private float _tBoss = 0f;
        private Coroutine _fade;

        private EventInstance _mainInst;
        private EventInstance _bossInst;

        private void Awake()
        {
            _box = GetComponent<BoxCollider>();
            _box.isTrigger = true;

            _armed = startArmed;

            if (useMainFromConfig)
            {
                var cfg = AudioResources.Config;
                if (cfg != null) mainMusic = cfg.mainMusicEvent;
            }
        }

        private void Start()
        {
            EnsureInstances();
            ApplyMixImmediate(1f, 0f);
        }

        // Called by a button / timeline / cutscene.
        public void ArmAndEnableBoss()
        {
            _armed = true;

            EnsureInstances();

            bool insideNow = IsActivatorInsideNow();
            _inside = insideNow;

            if (insideNow)
                SetTargets(0f, 1f, armFadeSeconds);
            else
                ApplyMixImmediate(1f, 0f);
        }

        public void DisarmAndReturnToMain()
        {
            _armed = false;
            _inside = false;

            SetTargets(1f, 0f, armFadeSeconds);
        }

        private void Update()
        {
            if (!_armed) return;

            bool insideNow = IsActivatorInsideNow();

            if (insideNow)
            {
                _lastInside = Time.time;

                if (!_inside)
                {
                    _inside = true;
                    SetTargets(0f, 1f, zoneFadeSeconds);
                }
            }
            else
            {
                if (_inside && (Time.time - _lastInside) >= exitGraceSeconds)
                {
                    _inside = false;
                    SetTargets(1f, 0f, zoneFadeSeconds);
                }
            }
        }

        private void EnsureInstances()
        {
            if (!mainMusic.IsNull && !_mainInst.isValid())
            {
                _mainInst = RuntimeManager.CreateInstance(mainMusic);
                _mainInst.start();
            }

            if (!bossMusic.IsNull && !_bossInst.isValid())
            {
                _bossInst = RuntimeManager.CreateInstance(bossMusic);
                _bossInst.start();
            }
        }

        private bool IsActivatorInsideNow()
        {
            Vector3 center = transform.TransformPoint(_box.center);
            Vector3 half = Vector3.Scale(_box.size, transform.lossyScale) * 0.5f;

            Collider[] hits = Physics.OverlapBox(center, half, transform.rotation, activatorLayers, QueryTriggerInteraction.Ignore);
            return hits != null && hits.Length > 0;
        }

        private void SetTargets(float main, float boss, float seconds)
        {
            _tMain = Mathf.Clamp01(main);
            _tBoss = Mathf.Clamp01(boss);

            if (_fade != null) StopCoroutine(_fade);
            _fade = StartCoroutine(Fade(seconds));
        }

        private IEnumerator Fade(float seconds)
        {
            float sMain = _curMain;
            float sBoss = _curBoss;

            if (seconds <= 0f)
            {
                ApplyMixImmediate(_tMain, _tBoss);
                _fade = null;
                yield break;
            }

            float t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime;
                float a = Mathf.Clamp01(t / seconds);
                a = a * a * (3f - 2f * a);

                _curMain = Mathf.Lerp(sMain, _tMain, a);
                _curBoss = Mathf.Lerp(sBoss, _tBoss, a);

                ApplyMixImmediate(_curMain, _curBoss);
                yield return null;
            }

            ApplyMixImmediate(_tMain, _tBoss);
            _fade = null;
        }

        private void ApplyMixImmediate(float main, float boss)
        {
            _curMain = Mathf.Clamp01(main);
            _curBoss = Mathf.Clamp01(boss);

            if (_mainInst.isValid() && !string.IsNullOrEmpty(mainAudibleParam))
                _mainInst.setParameterByName(mainAudibleParam, _curMain);

            if (_bossInst.isValid() && !string.IsNullOrEmpty(bossAudibleParam))
                _bossInst.setParameterByName(bossAudibleParam, _curBoss);
        }

        private void OnDestroy()
        {
            StopInst(ref _mainInst);
            StopInst(ref _bossInst);
        }

        private static void StopInst(ref EventInstance inst)
        {
            if (!inst.isValid()) return;
            inst.stop(STOP_MODE.IMMEDIATE);
            inst.release();
            inst.clearHandle();
        }
    }
}
