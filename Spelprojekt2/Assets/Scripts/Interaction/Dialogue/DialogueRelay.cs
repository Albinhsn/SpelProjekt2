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
        [CanBeNull] private DialogueSender m_activeSender = null;

        public void Initiate(Story story, DialogueSender sender)
        {
            if (m_activeSender is not null && m_activeSender != sender)
            {
                
            }
            d_onDialogueInitiation?.Invoke(story);
        }

        public void Select(int alt)
        {
            m_activeSender.Select(alt);
        }
        
    }
}