using System;
using System.Collections.Generic;
using Ink.Runtime;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Interaction.Dialogue
{
    public class DialogueReceiver : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private DialogueRelay m_relay;
        [SerializeField] private FontSet m_fontSet; //Index 0 is default
        
        [Header("Local")]
        [SerializeField] private GameObject m_dialogueContainer;
        [SerializeField] private TextMeshProUGUI m_textOut;
        [SerializeField] private GameObject m_canContinueIndicator;
        [SerializeField] private Transform m_altButtonContainer;
        [SerializeField] private GameObject m_altButtonPrefab;
        [SerializeField] private float m_altButtonOffset;
        private List<Button> m_altButtons;
        [CanBeNull] private Story m_activeStory;

        private void OnEnable()
        {
            m_relay.d_onDialogueInitiation += InitiateDialogue;
            m_relay.d_onDialogueUpdate += UpdateDialogue;
            m_relay.d_onDialogueExit += EndDialogue;
            m_dialogueContainer.SetActive(false);
            if (m_altButtons is null) m_altButtons = new List<Button>();
        }
        private void OnDisable()
        {
            m_relay.d_onDialogueInitiation -= InitiateDialogue;
            m_relay.d_onDialogueUpdate -= UpdateDialogue;
            m_relay.d_onDialogueExit -= EndDialogue;
        }

        private void InitiateDialogue(Story story)
        {
            SetFont(0, m_textOut);
            Assert.IsTrue(m_activeStory is null);
            m_dialogueContainer.SetActive(true);
            m_activeStory = story;
            UpdateDialogue();
        }
        private void UpdateDialogue()
        {
            ParseTags(m_activeStory.currentTags.ToArray());
            Debug.Log(m_activeStory.currentText);
            m_textOut.text = m_activeStory.currentText;
            SetAlternatives(m_activeStory.currentChoices.ToArray());
            m_canContinueIndicator.SetActive(m_activeStory.canContinue);
        }
        private void EndDialogue()
        {
            m_dialogueContainer.SetActive(false);
            m_activeStory = null;
        }
        public void ContinueAction()
        {
            m_relay.Select(-1);
        }

        private void SetAlternatives(Choice[] alts)
        {
            for (int a = m_altButtons.Count; a < alts.Length; a++)
            {
                m_altButtons.Add(Instantiate(m_altButtonPrefab, m_altButtonContainer).GetComponent<Button>());
                RectTransform alt_transform = m_altButtons[a].GetComponent<RectTransform>();
                alt_transform.anchoredPosition = new Vector2(0, 0 - alt_transform.rect.height / 2 - m_altButtonOffset - a * alt_transform.rect.height);
                int alt = a;
                m_altButtons[a].onClick.RemoveAllListeners();//Unity moment
                m_altButtons[a].onClick.AddListener(() => m_relay.Select(alt));
                SetFont(0, m_altButtons[a].GetComponentInChildren<TextMeshProUGUI>());
            }

            for (int a = 0; a < m_altButtons.Count; a++)
            {
                if (a < alts.Length)
                {
                    m_altButtons[a].gameObject.SetActive(true);
                    m_altButtons[a].GetComponentInChildren<TextMeshProUGUI>().text = alts[a].text;
                }
                else m_altButtons[a].gameObject.SetActive(false);
            }
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
                        case "font":
                            SetFont(value, m_textOut);//Speaker font
                            break;
                    }
                }
            }
        }

        private void SetFont(int font_index, TextMeshProUGUI target)
        {
            Assert.IsTrue(font_index < m_fontSet.count);
            target.font = m_fontSet.GetFont(font_index);
        }
    }
}