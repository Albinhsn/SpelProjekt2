using System;
using System.Collections.Generic;
using Ink.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Interaction.Dialogue
{
    public class DialogueReceiver : MonoBehaviour
    {
        [SerializeField] private DialogueRelay m_relay;
        [SerializeField] private TextMeshPro m_textOut;
        [SerializeField] private Transform m_altButtonContainer;
        [SerializeField] private GameObject m_altButtonPrefab;
        [SerializeField] private float m_altButtonOffset;
        private List<Button> m_altButtons;
        private Story m_activeStory;

        private void Awake()
        {
            
        }

        private void InitiateDialogue(Story story)
        {
            m_activeStory = story;
        }
        private void UpdateDialogue()
        {
            SetAlternatives(m_activeStory.currentChoices);
        }
        private void EndDialogue()
        {
            
        }
        private void ContinueAction()
        {
            m_relay.Select(-1);
        }

        private void SetAlternatives(Choice[] alts)
        {
            for (int a = m_altButtons.Count; a < alts.Length; a++)
            {
                m_altButtons.Add(Instantiate(m_altButtonPrefab, m_altButtonContainer).GetComponent<Button>());
                int alt = a;
                m_altButtons[a].onClick.RemoveAllListeners();//Unity moment
                m_altButtons[a].onClick.AddListener(() => m_relay.Select(alt));
            }

            for (int a = 0; a < m_altButtons.Count; a++)
            {
                if (a < alts.Length)
                {
                    m_altButtons[a].gameObject.SetActive(true);
                    m_altButtons[a].GetComponentInChildren<TextMeshPro>().text = alts[a];
                }
                else m_altButtons[a].gameObject.SetActive(false);
            }
        }
    }
}