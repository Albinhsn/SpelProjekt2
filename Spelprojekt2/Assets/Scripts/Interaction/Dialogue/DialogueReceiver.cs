using System;
using System.Collections.Generic;
using AudioKit.FMOD;
using Ink.Runtime;
using JetBrains.Annotations;
using TMPro;
using Unity.Cinemachine;
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
        [SerializeField] private float m_inputRepeatDelay = 0.1f;
        [SerializeField] private string m_speakerSoundEvent = "dsSpeakingSound";
        [SerializeField] private float m_fontSizeAt1080;
        [SerializeField] private float m_textOutNoAltOffset = 200;
        [SerializeField] private Image m_textContainer;
        [SerializeField] private Image[] m_altBGs;
        
        
        private TextMeshProUGUI m_textOut;
        private TextMeshProUGUI[] m_altTexts;
        private int m_currentAltCount;
        private int m_selectedAlt = 0;

        private float m_textOutDefaultPos;
        
        //Refs
        private CinemachineBrain m_camera;
        private float m_cameraGlobalDefaultBlendTime;
        private float m_cameraInteractionDefaultBlendTime;
        
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
        }

        void Start()
        {
            
            if (!m_subscribed && !m_initialSubscribed)
            {
                m_relay.d_onDialogueInitiation += InitiateDialogue;
                m_relay.d_onDialogueUpdate += UpdateDialogue;
                m_relay.d_onDialogueExit += EndDialogue;

                if (m_altTexts is null || m_altTexts.Length != m_altBGs.Length)
                {
                    m_altTexts = new TextMeshProUGUI[m_altBGs.Length];
                    for (int a = 0; a < m_altBGs.Length; a++)
                    {
                        m_altTexts[a] = m_altBGs[a].GetComponentInChildren<TextMeshProUGUI>();
                        m_altBGs[a].gameObject.SetActive(false);
                    }
                }

                m_textOut = m_textContainer.GetComponentInChildren<TextMeshProUGUI>();
                m_textContainer.gameObject.SetActive(false);
                m_textOutDefaultPos = m_textContainer.rectTransform.anchoredPosition.y;
                
                m_subscribed = true;
                m_initialSubscribed = true;
                m_textOut.fontSize = m_fontSizeAt1080 / 1080f * Screen.height;
                m_camera = FindFirstObjectByType<CinemachineBrain>();
                m_cameraGlobalDefaultBlendTime = m_camera.DefaultBlend.Time;
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
            if (!m_subscribed && !m_initialSubscribed)
            {
                m_relay.d_onDialogueInitiation += InitiateDialogue;
                m_relay.d_onDialogueUpdate += UpdateDialogue;
                m_relay.d_onDialogueExit += EndDialogue;

                if (m_altTexts is null || m_altTexts.Length != m_altBGs.Length)
                {
                    m_altTexts = new TextMeshProUGUI[m_altBGs.Length];
                    for (int a = 0; a < m_altBGs.Length; a++)
                    {
                        m_altTexts[a] = m_altBGs[a].GetComponentInChildren<TextMeshProUGUI>();
                        m_altBGs[a].gameObject.SetActive(false);
                    }
                }

                m_textOut = m_textContainer.GetComponentInChildren<TextMeshProUGUI>();
                m_textContainer.gameObject.SetActive(false);
                
                m_subscribed = true;
                m_initialSubscribed = true;
                m_textOut.fontSize = m_fontSizeAt1080 / 1080f * Screen.height;
                m_camera = FindFirstObjectByType<CinemachineBrain>();
                m_cameraGlobalDefaultBlendTime = m_camera.DefaultBlend.Time;
            }
            else if (!m_subscribed)
            {
                m_relay.d_onDialogueInitiation += InitiateDialogue;
                m_relay.d_onDialogueUpdate += UpdateDialogue;
                m_relay.d_onDialogueExit += EndDialogue;
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
            Assert.IsTrue(m_activeStory is null);
            
            m_textContainer.gameObject.SetActive(true);
            SetFont(0, m_textOut);
            
            m_prefix = "";
            m_activeStory = packet.story;
            m_typeDelay = packet.typeDelay;
            m_dfTypeDelay = packet.typeDelay;
            m_typingIndicator = packet.typingIndicator;
            m_cameraInteractionDefaultBlendTime = m_cameraGlobalDefaultBlendTime;

            if(m_activeStory.globalTags is not null) ParseGlobalTags(m_activeStory.globalTags.ToArray());
            UpdateDialogue();
            m_textContainer.rectTransform.anchoredPosition = new Vector2(m_textContainer.rectTransform.anchoredPosition.x, -m_textOutNoAltOffset);
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
        }
        private void EndDialogue()
        {
            m_textContainer.gameObject.SetActive(false);
            m_activeStory = null;
            m_prefix = "";

            m_camera.DefaultBlend = new CinemachineBlendDefinition(m_camera.DefaultBlend.Style, m_cameraGlobalDefaultBlendTime);
            for (int a = 0; a < m_altBGs.Length; a++)
            {
                m_altBGs[a].gameObject.SetActive(false);
            }
            UIManager.EnterState(UIState.None);
        }

        private void SetAlternatives(Choice[] alts)
        {
            m_currentAltCount = alts.Length;
            
            for (int a = m_altBGs.Length - 1; a > -1 ; a--)
            {
                if (a < m_currentAltCount)
                {
                    m_altBGs[a].gameObject.SetActive(true);
                    m_altTexts[a].text = alts[a].text;
                }
                else
                {
                    m_altBGs[a].gameObject.SetActive(false);
                }
            }

            if (m_currentAltCount == 0)
            {
                m_textContainer.rectTransform.anchoredPosition = new Vector2(m_textContainer.rectTransform.anchoredPosition.x, -m_textOutNoAltOffset);
                return;
            }
            
            m_textContainer.rectTransform.anchoredPosition = new Vector2(m_textContainer.rectTransform.anchoredPosition.x, m_textOutDefaultPos);
            
            m_selectedAlt = 0;
            m_lastDiscreteInput = 0;
            m_holdCooldown = 0;
            m_lastPointerPosition = InputManager.ReadPointerPosition();
            Highlight(m_altBGs[0]);
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
                    if (a == m_selectedAlt) Highlight(m_altBGs[a]);
                    else Unhighlight(m_altBGs[a]);
                }
            }
        }

        private void HandleAlternativeSelect()
        {
            if(m_selectedAlt == -1) return;

            if (InputManager.SelectUIOption() ||
                InputManager.UIPointerSelect() && m_selectedAlt == GetPointerAltSector())
            {
                Unhighlight(m_altBGs[m_selectedAlt]);
                for (int a = 0; a < m_currentAltCount; a++)
                {
                    m_altBGs[a].gameObject.SetActive(false);
                }
                m_relay.Select(m_selectedAlt);
            }
        }

        private int GetPointerAltSector()
        {
            Vector2 pointer_position = InputManager.ReadPointerPosition();
            for (int a = 0; a < m_currentAltCount; a++)
            {
                Rect bounds = m_altBGs[a].rectTransform.rect;
                bounds.center = m_altBGs[a].rectTransform.position;
                if (bounds.Contains(pointer_position)) return a;
            }
            return -1;
        }

        private void Highlight(Image text)
        {
            text.color = m_colorSet.alternativeHighlightColor;
        }
        private void Unhighlight(Image text)
        {
            text.color = m_colorSet.alternativeDefaultColor;
        }

        private void ParseTags(string[] tags)//Tag syntax: "{tag}={value}" or "{static tag}"
        {
            bool use_default_color = true;
            bool use_default_transition_time = true;
            
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
                        case "transition":
                            m_camera.DefaultBlend = new CinemachineBlendDefinition(m_camera.DefaultBlend.Style, float.Parse(value));
                            use_default_transition_time = false;
                            break;
                        
                    }
                }
            }

            if (use_default_color) m_textOut.color = m_colorSet.textDefaultColor;
            if (use_default_transition_time) m_camera.DefaultBlend = new CinemachineBlendDefinition(m_camera.DefaultBlend.Style, m_cameraInteractionDefaultBlendTime);
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
                        case "transition":
                            m_cameraInteractionDefaultBlendTime = float.Parse(value);
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
