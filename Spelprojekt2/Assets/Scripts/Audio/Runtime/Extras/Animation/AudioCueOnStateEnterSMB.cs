using UnityEngine;

namespace SP2.Audio
{
    // Ersätter gamla "BossDeathSMB"-stilen men är generell.
    public sealed class AudioCueOnStateEnterSMB : StateMachineBehaviour
    {
        [Header("== Cue ==")]
        public AudioCueSO cue;

        [Header("== Options ==")]
        public bool play2D = false;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (cue == null) return;

            var sys = AudioSystem.Instance;
            if (sys == null) return;

            if (play2D || cue.is2D) sys.Sfx?.Play2D(cue);
            else sys.Sfx?.Play(cue, animator.transform.position);
        }
    }
}
