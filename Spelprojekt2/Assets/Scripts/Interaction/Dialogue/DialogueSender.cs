using System;
using Ink.Runtime;
using JetBrains.Annotations;
using UnityEngine;

namespace Interaction.Dialogue
{
    public class DialogueSender : MonoBehaviour
    {
        [SerializeField] private TextAsset m_inkData;
        [SerializeField] private DialogueRelay m_dlsRelay;
        private Story m_story;

        private void Awake()
        {
            m_story = new Story(m_inkData.text);
        }

        public void StartDialogue(Interactor interactor)
        {
            m_dlsRelay.Initiate(m_story, this);
        }

        public void Select(int selection)//-1 = continue, 0+ = alt
        {
            
        }
    }
}