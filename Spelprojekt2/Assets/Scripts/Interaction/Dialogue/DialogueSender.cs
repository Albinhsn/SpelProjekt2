using System;
using Ink.Runtime;
using JetBrains.Annotations;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;

namespace Interaction.Dialogue
{
    public class DialogueSender : MonoBehaviour
    {
        [SerializeField] private TextAsset m_inkData;
        [SerializeField] private DialogueRelay m_dlsRelay;
        [Tooltip("Index 0 is default while interacting")][SerializeField] private CinemachineCamera[] m_cameraOverrides;
        private Story m_story;
        private bool m_interacting;
        [CanBeNull] private Interactor m_activeInteractor;

        private void OnValidate()
        {
            Assert.IsTrue(m_cameraOverrides.Length>0, "Dialogue senders must have at least one camera override");
        }

        private void Awake()
        {
            m_story = new Story(m_inkData.text);
            foreach (CinemachineCamera obj in m_cameraOverrides)
            {
                obj.Priority = 0;
            }
        }

        public void StartDialogue(Interactor interactor)
        {
            if (!m_story.canContinue)
            {
                interactor.CancelInteract();
                return;
            }
            m_activeInteractor = interactor;
            SetActiveCamera(0);
            
            m_interacting = true;
            m_story.Continue();
            m_dlsRelay.Initiate(m_story, this);
        }
        
        private void UpdateDialogue()
        {
            ParseTags(m_story.currentTags.ToArray());
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
                        ExitDialogue();
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
                        ExitDialogue();
                        return;
                    }
                    m_story.Continue();
                    UpdateDialogue();
                    break;
            }
        }

        private void ExitDialogue()
        {
            m_dlsRelay.ExitDialogue();
            SetActiveCamera(-1);
            m_interacting = false;
            m_activeInteractor.CancelInteract();
            m_activeInteractor = null;
        }
        
        private void ParseTags(string[] tags)//Tag syntax: "{tag}={value}" or "{static tag}"
        {
            for (int a = 0; a < tags.Length; a++)
            {
                int value_index = tags[a].IndexOf('=');
                if (value_index == -1)//Static tag
                {
                    //switch for static tags
                }
                else
                {
                    int value = int.Parse(tags[a].Substring(value_index + 1));
                    switch (tags[a].Substring(0, value_index))
                    {
                        case "camera":
                            SetActiveCamera(value);
                            break;
                    }
                }
            }
        }

        private void SetActiveCamera(int index) //Index -1 returns to player camera
        {
            for (int a = 0; a < m_cameraOverrides.Length; a++)
            {
                m_cameraOverrides[a].Priority = a == index ? int.MaxValue : 0;
            }
        }
    }
}