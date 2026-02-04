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
        private bool m_interacting;
        [CanBeNull] private Interactor m_activeInteractor;

        private void Awake()
        {
            m_story = new Story(m_inkData.text);
        }

        public void StartDialogue(Interactor interactor)
        {
            if (!m_story.canContinue)
            {
                interactor.CancelInteract();
                return;
            }
            m_activeInteractor = interactor;
            
            m_interacting = true;
            m_story.Continue();
            m_dlsRelay.Initiate(m_story, this);
        }
        
        private void UpdateDialogue()
        {
            ExecuteTagActions(m_story.currentTags.ToArray());
            m_dlsRelay.UpdateDialogue();
        }

        public void Select(int selection)//-1 = continue, -2 = abort, 0+ = alt
        {
            switch (selection)
            {
                case -1:
                    if (m_story.canContinue)
                    {
                        m_story.Continue();
                        UpdateDialogue();
                    }
                    else if (m_story.currentChoices.Count == 0)//No more choices
                    {
                        m_dlsRelay.ExitDialogue();
                        m_interacting = false;
                        m_activeInteractor.CancelInteract();
                        m_activeInteractor = null;
                        return;
                    }
                    break;
                case -2:
                    m_interacting = false;
                    break;
                default:
                    m_story.ChooseChoiceIndex(selection);
                    m_story.Continue();//Continue twice to skip reading own choice
                    if (!m_story.canContinue)//Exit
                    {
                        m_dlsRelay.ExitDialogue();
                        m_interacting = false;
                        m_activeInteractor.CancelInteract();
                        m_activeInteractor = null;
                        return;
                    }
                    m_story.Continue();
                    UpdateDialogue();
                    break;
            }
        }
        
        private void ExecuteTagActions(string[] tags)
        {
            for (int a = 0; a < tags.Length; a++)
            {
                switch (tags[a])
                {//TODO: add world tags here
                    default:{break;}
                }
            }
        }
    }
}
