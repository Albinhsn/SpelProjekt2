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
        [SerializeField] private DialogueColorSet m_colorSet;
        
        [Header("Local")]
        [SerializeField] private GameObject m_dialogueContainer;
        [SerializeField] private TextMeshProUGUI m_textOut;
        [SerializeField] private GameObject m_canContinueIndicator;
        [SerializeField] private Transform m_altButtonContainer;
        [SerializeField] private GameObject m_altButtonPrefab;
        [SerializeField] private float m_altButtonOffset;
        private List<Button> m_altButtons;
        
        //Per-interaction
        [CanBeNull] private Story m_activeStory;
        private string m_speakerName;

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
            m_speakerName = "";
            SetFont(0, m_textOut);
            Assert.IsTrue(m_activeStory is null);
            m_dialogueContainer.SetActive(true);
            m_activeStory = story;
            if(m_activeStory.globalTags is not null) ParseGlobalTags(m_activeStory.globalTags.ToArray());
            UpdateDialogue();
        }
        private void UpdateDialogue()
        {
            if(m_activeStory.currentTags is not null) ParseTags(m_activeStory.currentTags.ToArray());
            Debug.Log(m_activeStory.currentText);
            
            if (m_speakerName != "")
            {
                m_textOut.text = m_speakerName + ": " + m_activeStory.currentText;
            }
            else m_textOut.text = m_activeStory.currentText;
            
            if(m_activeStory.currentChoices is not null)SetAlternatives(m_activeStory.currentChoices.ToArray());
            m_canContinueIndicator.SetActive(m_activeStory.canContinue);
        }
        private void EndDialogue()
        {
            m_dialogueContainer.SetActive(false);
            m_activeStory = null;
            m_speakerName = "";
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
                m_altButtons[a].onClick.RemoveAllListeners(); //Unity moment
                m_altButtons[a].onClick.AddListener(() => m_relay.Select(alt));
                TextMeshProUGUI alt_text_out = m_altButtons[a].GetComponentInChildren<TextMeshProUGUI>();
                SetFont(0, alt_text_out);
                
                alt_text_out.color = Color.white;//Coloration, must use color drivers from Button component
                ColorBlock button_colors = new ColorBlock();
                button_colors.normalColor = m_colorSet.defaultColor;
                button_colors.selectedColor = m_colorSet.defaultColor;
                button_colors.highlightedColor = m_colorSet.highlightColor;
                button_colors.pressedColor = m_colorSet.highlightColor;
                button_colors.colorMultiplier = 1;
                m_altButtons[a].colors = button_colors;
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
            bool use_default_color = true;
            
            for (int a = 0; a < tags.Length; a++)
            {
                int value_index = tags[a].IndexOf('=');
                if (value_index == -1)//Static tag
                {
                    //Switch here
                }
                else
                {
                    string value = tags[a].Substring(value_index + 1);
                    switch (tags[a].Substring(0, value_index))
                    {
                        case "font":
                            SetFont(int.Parse(value), m_textOut);//Speaker font
                            break;
                        case "color":
                            m_textOut.color = m_colorSet.GetColor(value);//Color of speaker text
                            use_default_color = false;
                            break;
                        case "speaker":
                            m_speakerName = value;
                            break;
                    }
                }
            }

            if (use_default_color) m_textOut.color = m_colorSet.defaultColor;
        }

        private void ParseGlobalTags(string[] tags)
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
                        case "speaker":
                            m_speakerName = value;
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