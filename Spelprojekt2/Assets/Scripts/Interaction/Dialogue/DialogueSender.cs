using System;
using Ink.Runtime;
using JetBrains.Annotations;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

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
        [CanBeNull] private InkAssetRegistry m_assetRegistry = null;

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

            if (TryGetComponent(out InkAssetRegistry register)) m_assetRegistry = register;
        }

        public void StartDialogue(Interactor interactor)
        {
            if (m_assetRegistry is not null) m_story = new Story(m_assetRegistry.activeInkAsset.text);
            
            GlobalInkVariableManager.SyncDown(m_story);
            
            if (!m_story.canContinue)
            {
                interactor.FinishInteraction();
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
                    //m_story.Continue();//Continue twice to skip reading own choice
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
            GlobalInkVariableManager.SyncUp(m_story);
            m_dlsRelay.ExitDialogue();
            SetActiveCamera(-1);
            m_interacting = false;
            m_activeInteractor.FinishInteraction();
            m_activeInteractor = null;
            m_story.ResetState();
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
                    string value = tags[a].Substring(value_index + 1);
                    switch (tags[a].Substring(0, value_index))
                    {
                        case "camera":
                            SetActiveCamera(int.Parse(value));
                            break;
                        case "anim":
                        {
                            ParseAnimatorParameters(value);
                            break;
                        }
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

        [CanBeNull] private Animator m_animator;
        private void ParseAnimatorParameters(string parameters)
        {
            if (m_animator is null)
            {
                if (TryGetComponent(out Animator animator))
                {
                    m_animator = animator;
                }
                else Debug.LogError("Animation-containing ink data used on animatorless dialogue trigger", gameObject);
            }

            int segment_end_index = -1;
            do
            {
                int type_index = parameters.IndexOf(':', segment_end_index + 1);
                int value_index = parameters.IndexOf('=', segment_end_index + 1);
                string name = parameters.Substring(segment_end_index + 1, type_index - segment_end_index - 1);
                string type = parameters.Substring(type_index + 1, value_index - type_index - 1);
                segment_end_index = parameters.IndexOf(';', value_index);
                string value = parameters.Substring(value_index + 1, segment_end_index - value_index - 1);

                switch (type)
                {
                    case "int":
                        m_animator.SetInteger(name, int.Parse(value));
                        break;
                    case "float":
                        m_animator.SetFloat(name, float.Parse(value));
                        break;
                    case "bool":
                        m_animator.SetBool(name, bool.Parse(value));
                        break;
                }
            } while (segment_end_index < parameters.Length-1);

        }
    }
}