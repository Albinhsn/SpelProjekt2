using UnityEngine;

// AudioKit anteckning
// SMB helper för animationer
// Kan trigga slut på ett musikläge
// Valfritt att använda


namespace AudioKit.FMOD
{
    public sealed class EncounterEndSMB : StateMachineBehaviour
    {
        [Header("Söknamn")]
        [SerializeField] private string audioDriverComponentName = "EncounterMusicDriver";

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (animator == null) return;

            var driver = animator.GetComponentInParent<EncounterMusicDriver>();
            if (driver != null)
            {
                driver.EndEncounter();
            }
        }
    }
}
