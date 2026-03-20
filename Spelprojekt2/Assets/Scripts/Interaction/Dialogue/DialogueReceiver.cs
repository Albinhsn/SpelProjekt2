using System;
using System.Collections.Generic;
using AudioKit.FMOD;
using Ink.Runtime;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
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
        [SerializeField] private RectTransform m_dialogueContainer;
        [SerializeField] private TextMeshProUGUI m_textOut;
        [SerializeField] private RectTransform m_altButtonContainer;
        [SerializeField] private GameObject m_altButtonPrefab;
        [SerializeField] private float m_altButtonOffset;
        [SerializeField] private float m_inputRepeatDelay = 0.1f;
        [SerializeField] private string m_speakerSoundEvent = "dsSpeakingSound";
        [SerializeField] private float m_fontSizeAt1080;
        
        private List<TextMeshProUGUI> m_pooledAltObjects;
        private int m_currentAltCount;
        private int m_selectedAlt = 0;
        
        //Static
        private Vector2Int m_screenBounds;
        private Rect m_readOnlyAltTransform;
        
        //Per-interaction
        [CanBeNull] private Story m_activeStory;
        private bool m_subscribed;
        private bool m_initialSubscribed;
        private float m_dfTypeDelay;
        private float m_typeDelay;
        private float m_remainingTypeDelay;
        private string m_prefix;
        private char m_typingIndicator;
        private bool m_allowSkipTypeout;
        private bool m_altsChecked;
        
        private string m_activeText;
        private int m_activeCharIndex;

        
        private bool m_interacting => m_activeStory is not null;
        
        
        private void Awake()
        {
            m_subscribed = false;
            m_initialSubscribed = false;

            m_screenBounds = new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height);
            TextMeshProUGUI alt = Instantiate(m_altButtonPrefab, m_altButtonContainer).GetComponent<TextMeshProUGUI>();
            m_readOnlyAltTransform = alt.rectTransform.rect;
            Destroy(alt.gameObject);
        }

        void Start()
        {
            
            if (!m_subscribed && !m_initialSubscribed)
            {
                m_relay.d_onDialogueInitiation += InitiateDialogue;
                m_relay.d_onDialogueUpdate += UpdateDialogue;
                m_relay.d_onDialogueExit += EndDialogue;
                m_dialogueContainer.gameObject.SetActive(false);
                if (m_pooledAltObjects is null) m_pooledAltObjects = new List<TextMeshProUGUI>();
                m_subscribed = true;
                m_initialSubscribed = true;
                m_textOut.fontSize = m_fontSizeAt1080 / 1080f * Screen.height;
            }
        }

        private void Update()
        {
            if(!m_interacting) return;
            
            //Type out text
            if (m_activeCharIndex < m_activeText.Length)
            {
                if (m_allowSkipTypeout && (InputManager.UIAdvance() || InputManager.UIPointerSelect()))//Skip type out
                {
                    m_activeCharIndex = m_activeText.Length - 1;
                    SetAlternatives(m_activeStory.currentChoices.ToArray());
                    m_altsChecked = true;
                }

                m_remainingTypeDelay -= Time.deltaTime;
                if (m_remainingTypeDelay <= 0)
                {

                    if (m_activeText[m_activeCharIndex] == '<') m_activeCharIndex = m_activeText.IndexOf('>', m_activeCharIndex + 1);
                    
                    m_activeCharIndex++;
                    m_textOut.text = m_prefix + m_activeText.Substring(0, m_activeCharIndex);
                    if (m_activeCharIndex < m_activeText.Length) m_textOut.text += m_typingIndicator;
                    m_remainingTypeDelay = m_typeDelay;
                    
                    AudioEventHub.I.PlayOneShot(m_speakerSoundEvent, Vector3.zero);
                }
                
                return;
            }

            //Set alts and update positions after typeout finished
            if (!m_altsChecked)
            {
                SetAlternatives(m_activeStory.currentChoices.ToArray());
                m_altsChecked = true;
            }
            
            //Input handling
            if (m_currentAltCount == 0)//Continuation
            {
                if (InputManager.UIAdvance() || InputManager.UIPointerSelect()) m_relay.Select(-1);
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
                m_dialogueContainer.gameObject.SetActive(false);
                if (m_pooledAltObjects is null) m_pooledAltObjects = new List<TextMeshProUGUI>();
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

        private void InitiateDialogue(DialoguePacket packet)
        {
            UIManager.EnterState(UIState.Dialogue);
            
            m_prefix = "";
            SetFont(0, m_textOut);
            Assert.IsTrue(m_activeStory is null);
            m_dialogueContainer.gameObject.SetActive(true);
            
            m_activeStory = packet.story;
            m_typeDelay = packet.typeDelay;
            m_dfTypeDelay = packet.typeDelay;
            m_typingIndicator = packet.typingIndicator;

            if(m_activeStory.globalTags is not null) ParseGlobalTags(m_activeStory.globalTags.ToArray());
            UpdateDialogue();
        }
        private void UpdateDialogue()
        {
            m_selectedAlt = 0;
            m_currentAltCount = 0;
            m_allowSkipTypeout = true;
            if(m_activeStory.currentTags is not null) ParseTags(m_activeStory.currentTags.ToArray());
            
            
            m_textOut.text = m_prefix;
            m_activeCharIndex = 0;
            m_remainingTypeDelay = m_typeDelay;
            m_altsChecked = false;
            m_activeText = m_activeStory.currentText;

            m_textOut.rectTransform.anchoredPosition = new Vector2(m_textOut.rectTransform.anchoredPosition.x, -m_altButtonContainer.rect.height + m_textOut.rectTransform.rect.height);
            for (int a = 0; a < m_pooledAltObjects.Count; a++)
            {
                m_pooledAltObjects[a].gameObject.SetActive(false);
            }
        }
        private void EndDialogue()
        {
            m_dialogueContainer.gameObject.SetActive(false);
            m_activeStory = null;
            m_prefix = "";
            
            
            UIManager.EnterState(UIState.None);
        }

        private void SetAlternatives(Choice[] alts)
        {
            if (alts.Length == 0)
            {
                return;
            }
            
            m_currentAltCount = alts.Length;
            for (int a = m_pooledAltObjects.Count; a < m_currentAltCount; a++)
            {
                m_pooledAltObjects.Add(Instantiate(m_altButtonPrefab, m_altButtonContainer).GetComponent<TextMeshProUGUI>());
                SetFont(0, m_pooledAltObjects[a]);
                
                m_pooledAltObjects[a].color = m_colorSet.alternativeDefaultColor;
            }

            for (int a = 0; a < m_pooledAltObjects.Count; a++)
            {
                if (a < m_currentAltCount)
                {
                    m_pooledAltObjects[a].gameObject.SetActive(true);
                    m_pooledAltObjects[a].text = alts[a].text;
                    m_pooledAltObjects[a].rectTransform.anchoredPosition = new Vector2(0, -m_altButtonContainer.rect.height + (m_readOnlyAltTransform.height + m_altButtonOffset + (m_altButtonOffset + m_readOnlyAltTransform.height) * (m_currentAltCount - a - 1)));
                }
                else m_pooledAltObjects[a].gameObject.SetActive(false);
            }
            
            
            m_selectedAlt = 0;
            m_lastDiscreteInput = 0;
            m_holdCooldown = 0;
            m_lastPointerPosition = InputManager.ReadPointerPosition();
            Highlight(m_pooledAltObjects[0]);
            
            m_textOut.rectTransform.anchoredPosition = new Vector2(m_textOut.rectTransform.anchoredPosition.x,
                -m_altButtonContainer.rect.height + (m_textOut.rectTransform.rect.height / 2 + m_readOnlyAltTransform.height + m_altButtonOffset + (m_altButtonOffset + m_readOnlyAltTransform.height) * m_currentAltCount));
        }

        private Vector2 m_lastPointerPosition;
        private int m_lastDiscreteInput = 0;
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
                int navigation_input = (int)MathF.Ceiling(-InputManager.ReadUINavigationValue().y);
                m_holdCooldown -= Time.deltaTime;
                if (navigation_input != 0 || m_lastDiscreteInput != navigation_input)
                {
                    if (m_holdCooldown <= 0 || m_lastDiscreteInput != navigation_input)
                    {
                        m_holdCooldown = m_inputRepeatDelay;
                        m_lastDiscreteInput = navigation_input;
                        m_selectedAlt += navigation_input;
                        //Wrap
                        if (m_selectedAlt >= m_currentAltCount) m_selectedAlt = 0;
                        else if (m_selectedAlt < 0) m_selectedAlt = m_currentAltCount - 1;
                    }
                }
            }

            //Visuals
            if (m_selectedAlt != prev_selection)
            {
                for (int a = 0; a < m_currentAltCount; a++)
                {
                    if (a == m_selectedAlt) Highlight(m_pooledAltObjects[a]);
                    else Unhighlight(m_pooledAltObjects[a]);
                }
            }
        }

        private void HandleAlternativeSelect()
        {
            if(m_selectedAlt == -1) return;

            if (InputManager.SelectUIOption() ||
                InputManager.UIPointerSelect() && m_selectedAlt == GetPointerAltSector())
            {
                Unhighlight(m_pooledAltObjects[m_selectedAlt]);
                m_relay.Select(m_selectedAlt);
            }
        }

        private int GetPointerAltSector()
        {
            Vector2 pointer_position = InputManager.ReadPointerPosition();
            for (int a = 0; a < m_currentAltCount; a++)
            {
                Rect bounds = m_pooledAltObjects[a].rectTransform.rect;
                bounds.center = m_pooledAltObjects[a].rectTransform.position;
                if (bounds.Contains(pointer_position)) return a;
            }
            return -1;
        }

        private void Highlight(TextMeshProUGUI text)
        {
            text.color = m_colorSet.alternativeHighlightColor;
        }
        private void Unhighlight(TextMeshProUGUI text)
        {
            text.color = m_colorSet.alternativeDefaultColor;
        }

        private void ParseTags(string[] tags)//Tag syntax: "{tag}={value}" or "{static tag}"
        {
            bool use_default_color = true;
            
            for (int a = 0; a < tags.Length; a++)
            {
                int value_index = tags[a].IndexOf('=');
                if (value_index == -1)//Static tag
                {
                    switch (tags[a])
                    {
                        case "disallowSkip":
                            m_allowSkipTypeout = false;
                            break;
                    }
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
                            if (value == "") m_prefix = "";
                            else m_prefix = value + ": ";
                            break;
                        case "delay":
                            if (value == "default") m_typeDelay = m_dfTypeDelay;
                            else m_typeDelay = float.Parse(value);
                            break;
                        
                    }
                }
            }

            if (use_default_color) m_textOut.color = m_colorSet.textDefaultColor;
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
                            if (value == "") m_prefix = "";
                            else m_prefix = value + ": ";
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
