using Ink.Runtime;
using JetBrains.Annotations;
using UnityEngine;

namespace Interaction.Dialogue
{
    [CreateAssetMenu(menuName = "Data/DialogueSysRelay")]
    public class DialogueRelay : ScriptableObject
    {
        [CanBeNull] public event System.Action<Story> d_onDialogueInitiation;
        [CanBeNull] public event System.Action d_onDialogueUpdate;
        [CanBeNull] public event System.Action d_onDialogueExit;
        [CanBeNull] private DialogueSender m_activeSender = null;

        public void Initiate(Story story, DialogueSender sender)
        {
            if (m_activeSender is not null && m_activeSender != sender)
            {
                return;
            }

            m_activeSender = sender;
            d_onDialogueInitiation?.Invoke(story);
        }

        public void UpdateDialogue() => d_onDialogueUpdate?.Invoke();
        public void ExitDialogue() => d_onDialogueExit?.Invoke();

        public void Select(int alt)
        {
            m_activeSender.Select(alt);
        }
        
    }
}