using System;
using Ink.Runtime;
using JetBrains.Annotations;
using srUtils.Unity;
using UnityEngine;

namespace Interaction.Dialogue
{
    [ExecuteAlways]
    [CreateAssetMenu(menuName = "Data/DialogueSystem/DialogueSysRelay")]
    public class DialogueRelay : ResettableScriptableObject
    {
        [CanBeNull] public event System.Action<Story> d_onDialogueInitiation;
        [CanBeNull] public event System.Action d_onDialogueUpdate;
        [CanBeNull] public event System.Action d_onDialogueExit;
        [CanBeNull] private DialogueSender m_activeSender = null;

        [Tooltip("Reset object (fixes broken links caused by incorrect game exit)")][SerializeField] private bool m_reset;

        private void OnValidate()
        {
            if (m_reset)
            {
                m_reset = false;
                Reset();
            }
        }

        public override void Reset()
        {
            d_onDialogueInitiation = null;
            d_onDialogueUpdate = null;
            d_onDialogueExit = null;
            m_activeSender = null;
        }

        public void Initiate(Story story, DialogueSender sender)
        {
            if (m_activeSender is not null && m_activeSender != sender)
            {
                Debug.Log("Can't initiate dialogue relay");
                return;
            }

            m_activeSender = sender;
            d_onDialogueInitiation?.Invoke(story);
        }

        public void UpdateDialogue() => d_onDialogueUpdate?.Invoke();
        public void ExitDialogue()
        {
            m_activeSender = null;
            d_onDialogueExit?.Invoke();
        }

        public void Select(int alt)
        {
            m_activeSender.Select(alt);
        }
        
    }
}
