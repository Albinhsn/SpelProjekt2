using System.Collections;
using UnityEngine;

// AudioKit anteckning
// Enkel combat flagga med tokens
// Delay innan den går ner igen
// Kan återanvändas som 'puzzle stress'


namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    public sealed class CombatAudioState : MonoBehaviour
    {
        [SerializeField] private float exitDelay = 1.5f;

        [Header("Parameter")]
        [Tooltip("Valfritt: om satt används denna istället för cfg.combatParam.")]
        [SerializeField] private AudioParameterSO parameter;
        [SerializeField] private float onValue = 1f;
        [SerializeField] private float offValue = 0f;

        private int tokens;
        private Coroutine exitRoutine;

        public void EnterCombat()
        {
            tokens++;
            if (tokens < 0) tokens = 0;

            if (exitRoutine != null)
            {
                StopCoroutine(exitRoutine);
                exitRoutine = null;
            }

            SetCombat(true);
        }

        public void ExitCombat()
        {
            tokens--;
            if (tokens < 0) tokens = 0;

            if (tokens == 0 && exitRoutine == null)
                exitRoutine = StartCoroutine(ExitAfterDelay());
        }

        private IEnumerator ExitAfterDelay()
        {
            yield return new WaitForSeconds(exitDelay);
            exitRoutine = null;

            if (tokens == 0)
                SetCombat(false);
        }

        private void SetCombat(bool active)
        {
            if (AudioSystem.I == null) return;

            var cfg = AudioResources.Config;
            if (cfg == null) return;

            var name = (parameter != null && parameter.IsValid) ? parameter.fmodName : cfg.combatParam;
            if (string.IsNullOrEmpty(name)) return;
            AudioSystem.I.SetGlobalParam(name, active ? onValue : offValue);
        }
    }
}
