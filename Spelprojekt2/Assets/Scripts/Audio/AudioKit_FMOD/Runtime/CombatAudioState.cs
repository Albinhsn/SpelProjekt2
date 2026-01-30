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

            AudioSystem.I.SetGlobalParam(cfg.combatParam, active ? 1f : 0f);
        }
    }
}
