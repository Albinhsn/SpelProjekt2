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
        [SerializeField] private float m_inputRepeatDelay = 0.1f;
        
        private List<TextMeshProUGUI> m_alternatives;
        private int m_altCount;
        private int m_selectedAlt = -1;
        
        //Static
        private Vector2Int m_screenBounds;
        
        //Per-interaction
        [CanBeNull] private Story m_activeStory;
        private string m_speakerName;
        private bool m_subscribed;
        private bool m_initialSubscribed;

        
        private bool m_interacting => m_activeStory is not null;
        
        
        private void Awake()
        {
            m_subscribed = false;
            m_initialSubscribed = false;

            m_screenBounds = new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height);

        }

        void Start()
        {
            
            if (!m_subscribed && !m_initialSubscribed)
            {
                m_relay.d_onDialogueInitiation += InitiateDialogue;
                m_relay.d_onDialogueUpdate += UpdateDialogue;
                m_relay.d_onDialogueExit += EndDialogue;
                m_dialogueContainer.SetActive(false);
                if (m_alternatives is null) m_alternatives = new List<TextMeshProUGUI>();
                m_subscribed = true;
                m_initialSubscribed = true;
            }
        }

        private void Update()//Input handling
        {
            if(!m_interacting) return;


            if (m_altCount == 0)//Continuation
            {
                if (InputManager.UIAdvance()) m_relay.Select(-1);
            }
            else
            {
                HandleAlternatives();
                HandleAlternativeSelect();
            }
            
        }

        private void OnEnable()
        {
            if (!m_subscribed && m_initialSubscribed)
            {
                m_relay.d_onDialogueInitiation += InitiateDialogue;
                m_relay.d_onDialogueUpdate += UpdateDialogue;
                m_relay.d_onDialogueExit += EndDialogue;
                m_dialogueContainer.SetActive(false);
                if (m_alternatives is null) m_alternatives = new List<TextMeshProUGUI>();
                m_subscribed = true;
                
            }
        }
        void OnDisable()
        {
            m_relay.d_onDialogueInitiation -= InitiateDialogue;
            m_relay.d_onDialogueUpdate -= UpdateDialogue;
            m_relay.d_onDialogueExit -= EndDialogue;
            m_subscribed = false;
            
        }

        void OnDestroy()
        {
            m_relay.d_onDialogueInitiation -= InitiateDialogue;
            m_relay.d_onDialogueUpdate -= UpdateDialogue;
            m_relay.d_onDialogueExit -= EndDialogue;
            m_subscribed = false;
            
        }

        private void InitiateDialogue(Story story)
        {
            UIManager.EnterState(UIState.Dialogue);
            
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
            m_selectedAlt = -1;
            m_altCount = 0;
            if(m_activeStory.currentTags is not null) ParseTags(m_activeStory.currentTags.ToArray());
            
            if (m_speakerName != "")
            {
                m_textOut.text = m_speakerName + ": " + m_activeStory.currentText;
            }
            else m_textOut.text = m_activeStory.currentText;
            
            if(m_activeStory.currentChoices is not null) SetAlternatives(m_activeStory.currentChoices.ToArray());
            m_canContinueIndicator.SetActive(m_activeStory.canContinue);
        }
        private void EndDialogue()
        {
            m_dialogueContainer.SetActive(false);
            m_activeStory = null;
            m_speakerName = "";
            
            
            UIManager.EnterState(UIState.None);
        }

        private void SetAlternatives(Choice[] alts)
        {
            m_altCount = alts.Length;
            m_selectedAlt = -1;
            for (int a = m_alternatives.Count; a < m_altCount; a++)
            {
                m_alternatives.Add(Instantiate(m_altButtonPrefab, m_altButtonContainer).GetComponent<TextMeshProUGUI>());
                RectTransform alt_transform = m_alternatives[a].GetComponent<RectTransform>();
                alt_transform.anchoredPosition = new Vector2(0, 0 - alt_transform.rect.height / 2 - m_altButtonOffset - a * alt_transform.rect.height);
                SetFont(0, m_alternatives[a]);
                
                m_alternatives[a].color = m_colorSet.defaultColor;
            }

            for (int a = 0; a < m_alternatives.Count; a++)
            {
                if (a < m_altCount)
                {
                    m_alternatives[a].gameObject.SetActive(true);
                    m_alternatives[a].GetComponentInChildren<TextMeshProUGUI>().text = alts[a].text;
                }
                else m_alternatives[a].gameObject.SetActive(false);
            }
        }

        private Vector2 m_lastPointerPosition;
        private int m_lastDiscreteInput;
        private float m_holdCooldown;
        private void HandleAlternatives()
        {
            //Input handling
            int prev_selection = m_selectedAlt;
            Vector2 pointer_input = InputManager.ReadPointerPosition();
            if (m_lastPointerPosition != pointer_input) //Use pointer input only if pointer has moved. Enables discrete input
            {
                m_selectedAlt = GetPointerAltSector();
                m_lastPointerPosition = pointer_input;
                
            }
            else
            {
                //Discrete navigation input
                int navigation_input = (int)MathF.Ceiling(InputManager.ReadUINavigationValue().y);
                m_holdCooldown -= Time.deltaTime;
                if (m_holdCooldown <= 0 || m_lastDiscreteInput != navigation_input)
                {
                    m_holdCooldown = m_inputRepeatDelay;
                    m_lastDiscreteInput = navigation_input;
                    m_selectedAlt += navigation_input;
                    //Wrap
                    if (m_selectedAlt >= m_altCount) m_selectedAlt = 0;
                    else if (m_selectedAlt < 0) m_selectedAlt = m_altCount - 1;
                }
            }

            //Visuals
            if (m_selectedAlt != prev_selection)
            {
                for (int a = 0; a < m_altCount; a++)
                {
                    if (a == m_selectedAlt) Highlight(m_alternatives[a]);
                    else Unhighlight(m_alternatives[a]);
                }
            }
        }

        private void HandleAlternativeSelect()
        {
            if(m_selectedAlt == -1) return;

            if (InputManager.SelectUIOption() || InputManager.UIPointerSelect() && m_selectedAlt == GetPointerAltSector()) m_relay.Select(m_selectedAlt);

        }

        private int GetPointerAltSector()
        {
            //TODO: Mouse logic
            return -1;
        }

        private void Highlight(TextMeshProUGUI text)
        {
            text.color = m_colorSet.highlightColor;
        }
        private void Unhighlight(TextMeshProUGUI text)
        {
            text.color = m_colorSet.defaultColor;
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
