using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndCreditsManager : MonoBehaviour
{
    public enum PresentationMode
    {
        CardsFade,
        CardsTypewriter,
        StandardCrawl,
        StarWarsCrawl
    }

    public enum EntryType
    {
        Text,
        Image
    }

    [Serializable]
    public class CreditEntry
    {
        public EntryType entryType = EntryType.Text;

        [TextArea(2, 8)]
        public string text;

        public Sprite image;
        public bool preserveAspect = true;

        [Min(0f)]
        public float extraHoldTime = 0f;
    }

    [Header("References")]
    [SerializeField] private CanvasGroup m_contentCanvasGroup;
    [SerializeField] private TMP_Text m_creditsText;
    [SerializeField] private Image m_creditsImage;

    [Header("Presentation")]
    [SerializeField] private PresentationMode m_presentationMode = PresentationMode.CardsFade;

    [Header("Cards Settings")]
    [SerializeField] private CreditEntry[] m_credits;
    [Min(0.1f)]
    [SerializeField] private float m_totalCardsDuration = 30f;
    [Range(0.05f, 0.45f)]
    [SerializeField] private float m_fadePercentPerEntry = 0.25f;
    [Min(1f)]
    [SerializeField] private float m_lettersPerSecond = 35f;

    [Header("Standard Crawl Settings")]
    [TextArea(8, 20)]
    [SerializeField] private string m_standardCrawlText;
    [Min(0.1f)]
    [SerializeField] private float m_standardCrawlDuration = 20f;
    [SerializeField] private Vector2 m_standardCrawlStartPosition = new Vector2(0f, -500f);
    [SerializeField] private Vector2 m_standardCrawlEndPosition = new Vector2(0f, 1100f);
    [Min(0f)]
    [SerializeField] private float m_standardCrawlEndFadeDuration = 0.75f;

    [Header("Star Wars Crawl Settings")]
    [TextArea(8, 20)]
    [SerializeField] private string m_starWarsText;
    [Min(0.1f)]
    [SerializeField] private float m_starWarsDuration = 20f;
    [SerializeField] private Vector2 m_starWarsStartPosition = new Vector2(0f, -500f);
    [SerializeField] private Vector2 m_starWarsEndPosition = new Vector2(0f, 1200f);
    [SerializeField] private float m_starWarsTiltX = 55f;
    [Min(0.05f)]
    [SerializeField] private float m_starWarsStartScale = 1f;
    [Min(0.01f)]
    [SerializeField] private float m_starWarsEndScale = 0.25f;
    [Range(0f, 1f)]
    [SerializeField] private float m_starWarsFadeStartNormalized = 0.72f;

    [Header("Shared Timing")]
    [Min(0f)]
    [SerializeField] private float m_startDelay = 0.5f;
    [Min(0f)]
    [SerializeField] private float m_sceneChangeDelayAfterCredits = 0.5f;
    [SerializeField] private bool m_useUnscaledTime = true;

    [SerializeField] private LevelData m_nextLevel;

    private bool m_isFinishing;
    private RectTransform m_textRect;
    private Vector2 m_initialAnchoredPosition;
    private Quaternion m_initialLocalRotation;
    private Vector3 m_initialLocalScale;

    private void Awake()
    {
        if (m_contentCanvasGroup == null)
        {
            Debug.LogError("[EndCreditsManager] Missing CanvasGroup reference.");
            enabled = false;
            return;
        }

        if (m_creditsText == null)
        {
            Debug.LogError("[EndCreditsManager] Missing TMP_Text reference.");
            enabled = false;
            return;
        }

        m_textRect = m_creditsText.rectTransform;
        m_initialAnchoredPosition = m_textRect.anchoredPosition;
        m_initialLocalRotation = m_textRect.localRotation;
        m_initialLocalScale = m_textRect.localScale;

        if (ModeUsesCardEntries() && ContainsImageEntries() && m_creditsImage == null)
        {
            Debug.LogError("[EndCreditsManager] Missing Image reference, but one or more credit entries use Image.");
            enabled = false;
            return;
        }

        ClearVisuals();
        SetCanvasAlphaImmediate(0f);
    }

    public void StartCredits()
    {
        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        yield return WaitForSecondsCustom(m_startDelay);

        switch (m_presentationMode)
        {
            case PresentationMode.CardsFade:
                yield return PlayCardsFade();
                break;

            case PresentationMode.CardsTypewriter:
                yield return PlayCardsTypewriter();
                break;

            case PresentationMode.StandardCrawl:
                yield return PlayStandardCrawl();
                break;

            case PresentationMode.StarWarsCrawl:
                yield return PlayStarWarsCrawl();
                break;
        }

        yield return WaitForSecondsCustom(m_sceneChangeDelayAfterCredits);
        LoadNextScene();
    }

    private IEnumerator PlayCardsFade()
    {
        if (m_credits == null || m_credits.Length == 0)
        {
            yield return WaitForSecondsCustom(m_totalCardsDuration);
            yield break;
        }

        float totalExtraHold = GetTotalExtraHoldTime();
        float baseDurationPool = Mathf.Max(0.01f, m_totalCardsDuration - totalExtraHold);
        float entryDuration = baseDurationPool / m_credits.Length;
        float fadeDuration = Mathf.Clamp(entryDuration * m_fadePercentPerEntry, 0.05f, entryDuration * 0.45f);
        float holdDuration = Mathf.Max(0f, entryDuration - (fadeDuration * 2f));

        for (int i = 0; i < m_credits.Length; i++)
        {
            CreditEntry entry = m_credits[i];

            if (entry == null)
            {
                continue;
            }

            SetEntry(entry);

            float currentHold = holdDuration + Mathf.Max(0f, entry.extraHoldTime);

            yield return FadeCanvas(0f, 1f, fadeDuration);
            yield return WaitForSecondsCustom(currentHold);
            yield return FadeCanvas(1f, 0f, fadeDuration);
        }

        ClearVisuals();
    }

    private IEnumerator PlayCardsTypewriter()
    {
        if (m_credits == null || m_credits.Length == 0)
        {
            yield return WaitForSecondsCustom(m_totalCardsDuration);
            yield break;
        }

        float totalExtraHold = GetTotalExtraHoldTime();
        float baseDurationPool = Mathf.Max(0.01f, m_totalCardsDuration - totalExtraHold);
        float entryDuration = baseDurationPool / m_credits.Length;
        float fadeDuration = Mathf.Clamp(entryDuration * m_fadePercentPerEntry, 0.05f, entryDuration * 0.35f);
        float visibleDuration = Mathf.Max(0f, entryDuration - (fadeDuration * 2f));

        for (int i = 0; i < m_credits.Length; i++)
        {
            CreditEntry entry = m_credits[i];

            if (entry == null)
            {
                continue;
            }

            float currentVisibleDuration = visibleDuration + Mathf.Max(0f, entry.extraHoldTime);

            ClearVisuals();
            SetCanvasAlphaImmediate(0f);

            if (entry.entryType == EntryType.Image)
            {
                ShowImageEntry(entry);

                yield return FadeCanvas(0f, 1f, fadeDuration);
                yield return WaitForSecondsCustom(currentVisibleDuration);
                yield return FadeCanvas(1f, 0f, fadeDuration);
            }
            else
            {
                yield return PlayTypewriterTextEntry(entry.text, fadeDuration, currentVisibleDuration);
            }
        }

        ClearVisuals();
    }

    private IEnumerator PlayTypewriterTextEntry(string text, float fadeDuration, float visibleDuration)
    {
        PrepareTypewriterText(text);

        int totalVisibleCharacters = GetCurrentVisibleCharacterCount();
        float desiredTypeDuration = GetDesiredTypeDuration(text);
        float actualTypeDuration = Mathf.Min(desiredTypeDuration, visibleDuration);
        float holdAfterType = Mathf.Max(0f, visibleDuration - actualTypeDuration);

        float elapsed = 0f;
        float combinedDuration = Mathf.Max(fadeDuration, actualTypeDuration);

        if (combinedDuration <= 0f)
        {
            SetCanvasAlphaImmediate(1f);
            m_creditsText.maxVisibleCharacters = totalVisibleCharacters;
            yield return WaitForSecondsCustom(holdAfterType);
            yield return FadeCanvas(1f, 0f, fadeDuration);
            yield break;
        }

        while (elapsed < combinedDuration)
        {
            elapsed += GetDeltaTime();

            if (fadeDuration <= 0f)
            {
                SetCanvasAlphaImmediate(1f);
            }
            else
            {
                float fadeT = Mathf.Clamp01(elapsed / fadeDuration);
                SetCanvasAlphaImmediate(Mathf.Lerp(0f, 1f, fadeT));
            }

            if (actualTypeDuration <= 0f)
            {
                m_creditsText.maxVisibleCharacters = totalVisibleCharacters;
            }
            else
            {
                float typeT = Mathf.Clamp01(elapsed / actualTypeDuration);
                int visibleCount = Mathf.Clamp(
                    Mathf.FloorToInt(totalVisibleCharacters * typeT),
                    0,
                    totalVisibleCharacters
                );

                m_creditsText.maxVisibleCharacters = visibleCount;
            }

            yield return null;
        }

        SetCanvasAlphaImmediate(1f);
        m_creditsText.maxVisibleCharacters = totalVisibleCharacters;

        yield return WaitForSecondsCustom(holdAfterType);
        yield return FadeCanvas(1f, 0f, fadeDuration);
    }

    private IEnumerator PlayStandardCrawl()
    {
        ClearVisuals();
        ShowTextEntry(m_standardCrawlText);

        SetCanvasAlphaImmediate(1f);
        ResetTextTransform();

        m_textRect.anchoredPosition = m_standardCrawlStartPosition;
        m_textRect.localRotation = m_initialLocalRotation;
        m_textRect.localScale = m_initialLocalScale;

        float elapsed = 0f;

        while (elapsed < m_standardCrawlDuration)
        {
            elapsed += GetDeltaTime();
            float t = Mathf.Clamp01(elapsed / m_standardCrawlDuration);
            m_textRect.anchoredPosition = Vector2.Lerp(m_standardCrawlStartPosition, m_standardCrawlEndPosition, t);
            yield return null;
        }

        if (m_standardCrawlEndFadeDuration > 0f)
        {
            yield return FadeCanvas(1f, 0f, m_standardCrawlEndFadeDuration);
        }

        ClearVisuals();
    }

    private IEnumerator PlayStarWarsCrawl()
    {
        ClearVisuals();
        ShowTextEntry(m_starWarsText);

        ResetTextTransform();
        SetCanvasAlphaImmediate(1f);

        m_textRect.anchoredPosition = m_starWarsStartPosition;
        m_textRect.localRotation = Quaternion.Euler(m_starWarsTiltX, 0f, 0f);
        m_textRect.localScale = m_initialLocalScale * m_starWarsStartScale;

        float elapsed = 0f;

        while (elapsed < m_starWarsDuration)
        {
            elapsed += GetDeltaTime();
            float t = Mathf.Clamp01(elapsed / m_starWarsDuration);

            m_textRect.anchoredPosition = Vector2.Lerp(m_starWarsStartPosition, m_starWarsEndPosition, t);

            float currentScale = Mathf.Lerp(m_starWarsStartScale, m_starWarsEndScale, t);
            m_textRect.localScale = m_initialLocalScale * currentScale;

            float alpha = 1f;
            if (t >= m_starWarsFadeStartNormalized)
            {
                alpha = 1f - Mathf.InverseLerp(m_starWarsFadeStartNormalized, 1f, t);
            }

            SetCanvasAlphaImmediate(alpha);
            yield return null;
        }

        SetCanvasAlphaImmediate(0f);
        ClearVisuals();
    }

    private void SetEntry(CreditEntry entry)
    {
        ClearVisuals();

        if (entry == null)
        {
            return;
        }

        if (entry.entryType == EntryType.Text)
        {
            ShowTextEntry(entry.text);
        }
        else
        {
            ShowImageEntry(entry);
        }
    }

    private void ShowTextEntry(string text)
    {
        m_creditsText.gameObject.SetActive(true);
        m_creditsText.text = text ?? string.Empty;
        m_creditsText.maxVisibleCharacters = int.MaxValue;
        m_creditsText.ForceMeshUpdate();
    }

    private void PrepareTypewriterText(string text)
    {
        m_creditsText.gameObject.SetActive(true);
        m_creditsText.text = text ?? string.Empty;
        m_creditsText.maxVisibleCharacters = 0;
        m_creditsText.ForceMeshUpdate();
    }

    private int GetCurrentVisibleCharacterCount()
    {
        m_creditsText.ForceMeshUpdate();
        return m_creditsText.textInfo.characterCount;
    }

    private void ShowImageEntry(CreditEntry entry)
    {
        if (m_creditsImage == null)
        {
            Debug.LogError("[EndCreditsManager] Missing Image reference.");
            return;
        }

        m_creditsImage.gameObject.SetActive(true);
        m_creditsImage.sprite = entry.image;
        m_creditsImage.preserveAspect = entry.preserveAspect;
    }

    private void ClearVisuals()
    {
        if (m_creditsText != null)
        {
            m_creditsText.text = string.Empty;
            m_creditsText.maxVisibleCharacters = int.MaxValue;
            m_creditsText.gameObject.SetActive(false);
        }

        if (m_creditsImage != null)
        {
            m_creditsImage.sprite = null;
            m_creditsImage.gameObject.SetActive(false);
        }

        ResetTextTransform();
    }

    private void ResetTextTransform()
    {
        if (m_textRect == null)
        {
            return;
        }

        m_textRect.anchoredPosition = m_initialAnchoredPosition;
        m_textRect.localRotation = m_initialLocalRotation;
        m_textRect.localScale = m_initialLocalScale;
    }

    private float GetDesiredTypeDuration(string fullText)
    {
        if (string.IsNullOrEmpty(fullText))
        {
            return 0f;
        }

        return fullText.Length / Mathf.Max(1f, m_lettersPerSecond);
    }

    private IEnumerator FadeCanvas(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            SetCanvasAlphaImmediate(to);
            yield break;
        }

        float elapsed = 0f;
        SetCanvasAlphaImmediate(from);

        while (elapsed < duration)
        {
            elapsed += GetDeltaTime();
            float t = Mathf.Clamp01(elapsed / duration);
            SetCanvasAlphaImmediate(Mathf.Lerp(from, to, t));
            yield return null;
        }

        SetCanvasAlphaImmediate(to);
    }

    private IEnumerator WaitForSecondsCustom(float duration)
    {
        if (duration <= 0f)
        {
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += GetDeltaTime();
            yield return null;
        }
    }

    private void SetCanvasAlphaImmediate(float alpha)
    {
        m_contentCanvasGroup.alpha = alpha;
    }

    private float GetTotalExtraHoldTime()
    {
        if (m_credits == null || m_credits.Length == 0)
        {
            return 0f;
        }

        float total = 0f;

        for (int i = 0; i < m_credits.Length; i++)
        {
            CreditEntry entry = m_credits[i];

            if (entry != null)
            {
                total += Mathf.Max(0f, entry.extraHoldTime);
            }
        }

        return total;
    }

    private bool ContainsImageEntries()
    {
        if (m_credits == null)
        {
            return false;
        }

        for (int i = 0; i < m_credits.Length; i++)
        {
            CreditEntry entry = m_credits[i];

            if (entry != null && entry.entryType == EntryType.Image)
            {
                return true;
            }
        }

        return false;
    }

    private bool ModeUsesCardEntries()
    {
        return m_presentationMode == PresentationMode.CardsFade ||
               m_presentationMode == PresentationMode.CardsTypewriter;
    }

    private float GetDeltaTime()
    {
        return m_useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
    }

    private void LoadNextScene()
    {
        if (m_isFinishing)
        {
            return;
        }

        m_isFinishing = true;

        LevelManager.m_onTransitionEnd += UIManager.SetupMainMenu;
        LevelManager.TransitionToSceneAsync(m_nextLevel, 1.0f);

    }
}

#if UNITY_EDITOR
[UnityEditor.CustomPropertyDrawer(typeof(EndCreditsManager.CreditEntry))]
internal sealed class EndCreditsManagerCreditEntryDrawer : UnityEditor.PropertyDrawer
{
    private const float Spacing = 2f;

    public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
    {
        UnityEditor.EditorGUI.BeginProperty(position, label, property);

        UnityEditor.SerializedProperty entryTypeProp = property.FindPropertyRelative("entryType");
        UnityEditor.SerializedProperty textProp = property.FindPropertyRelative("text");
        UnityEditor.SerializedProperty imageProp = property.FindPropertyRelative("image");
        UnityEditor.SerializedProperty preserveAspectProp = property.FindPropertyRelative("preserveAspect");
        UnityEditor.SerializedProperty extraHoldTimeProp = property.FindPropertyRelative("extraHoldTime");

        float lineHeight = UnityEditor.EditorGUIUtility.singleLineHeight;
        float y = position.y;

        Rect foldoutRect = new Rect(position.x, y, position.width, lineHeight);
        property.isExpanded = UnityEditor.EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            UnityEditor.EditorGUI.indentLevel++;
            y += lineHeight + Spacing;

            Rect typeRect = new Rect(position.x, y, position.width, lineHeight);
            UnityEditor.EditorGUI.PropertyField(typeRect, entryTypeProp);

            y += lineHeight + Spacing;

            EndCreditsManager.EntryType entryType =
                (EndCreditsManager.EntryType)entryTypeProp.enumValueIndex;

            if (entryType == EndCreditsManager.EntryType.Text)
            {
                float textHeight = UnityEditor.EditorGUI.GetPropertyHeight(textProp, true);
                Rect textRect = new Rect(position.x, y, position.width, textHeight);
                UnityEditor.EditorGUI.PropertyField(textRect, textProp, true);
                y += textHeight + Spacing;
            }
            else
            {
                float imageHeight = UnityEditor.EditorGUI.GetPropertyHeight(imageProp, true);
                Rect imageRect = new Rect(position.x, y, position.width, imageHeight);
                UnityEditor.EditorGUI.PropertyField(imageRect, imageProp, true);
                y += imageHeight + Spacing;

                Rect preserveRect = new Rect(position.x, y, position.width, lineHeight);
                UnityEditor.EditorGUI.PropertyField(preserveRect, preserveAspectProp);
                y += lineHeight + Spacing;
            }

            Rect holdRect = new Rect(position.x, y, position.width, lineHeight);
            UnityEditor.EditorGUI.PropertyField(holdRect, extraHoldTimeProp);

            UnityEditor.EditorGUI.indentLevel--;
        }

        UnityEditor.EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(UnityEditor.SerializedProperty property, GUIContent label)
    {
        float lineHeight = UnityEditor.EditorGUIUtility.singleLineHeight;
        float totalHeight = lineHeight;

        if (!property.isExpanded)
        {
            return totalHeight;
        }

        UnityEditor.SerializedProperty entryTypeProp = property.FindPropertyRelative("entryType");
        UnityEditor.SerializedProperty textProp = property.FindPropertyRelative("text");
        UnityEditor.SerializedProperty imageProp = property.FindPropertyRelative("image");

        totalHeight += lineHeight + Spacing;
        totalHeight += Spacing;

        EndCreditsManager.EntryType entryType =
            (EndCreditsManager.EntryType)entryTypeProp.enumValueIndex;

        if (entryType == EndCreditsManager.EntryType.Text)
        {
            totalHeight += UnityEditor.EditorGUI.GetPropertyHeight(textProp, true) + Spacing;
        }
        else
        {
            totalHeight += UnityEditor.EditorGUI.GetPropertyHeight(imageProp, true) + Spacing;
            totalHeight += lineHeight + Spacing;
        }

        totalHeight += lineHeight + Spacing;
        return totalHeight;
    }
}

[UnityEditor.CustomEditor(typeof(EndCreditsManager))]
internal sealed class EndCreditsManagerEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EndCreditsManager.PresentationMode mode =
            (EndCreditsManager.PresentationMode)serializedObject.FindProperty("m_presentationMode").enumValueIndex;

        DrawHeader("References");
        Draw("m_contentCanvasGroup");
        Draw("m_creditsText");

        if (mode == EndCreditsManager.PresentationMode.CardsFade ||
            mode == EndCreditsManager.PresentationMode.CardsTypewriter)
        {
            Draw("m_creditsImage");
        }

        UnityEditor.EditorGUILayout.Space();
        DrawHeader("Presentation");
        Draw("m_presentationMode");

        UnityEditor.EditorGUILayout.Space();

        if (mode == EndCreditsManager.PresentationMode.CardsFade ||
            mode == EndCreditsManager.PresentationMode.CardsTypewriter)
        {
            DrawHeader("Cards Settings");
            Draw("m_credits", true);
            Draw("m_totalCardsDuration");
            Draw("m_fadePercentPerEntry");

            if (mode == EndCreditsManager.PresentationMode.CardsTypewriter)
            {
                Draw("m_lettersPerSecond");
            }
        }
        else if (mode == EndCreditsManager.PresentationMode.StandardCrawl)
        {
            DrawHeader("Standard Crawl Settings");
            Draw("m_standardCrawlText", true);
            Draw("m_standardCrawlDuration");
            Draw("m_standardCrawlStartPosition");
            Draw("m_standardCrawlEndPosition");
            Draw("m_standardCrawlEndFadeDuration");
        }
        else if (mode == EndCreditsManager.PresentationMode.StarWarsCrawl)
        {
            DrawHeader("Star Wars Crawl Settings");
            Draw("m_starWarsText", true);
            Draw("m_starWarsDuration");
            Draw("m_starWarsStartPosition");
            Draw("m_starWarsEndPosition");
            Draw("m_starWarsTiltX");
            Draw("m_starWarsStartScale");
            Draw("m_starWarsEndScale");
            Draw("m_starWarsFadeStartNormalized");
        }

        UnityEditor.EditorGUILayout.Space();
        DrawHeader("Shared Timing");
        Draw("m_startDelay");
        Draw("m_sceneChangeDelayAfterCredits");
        Draw("m_useUnscaledTime");

        UnityEditor.EditorGUILayout.Space();
        DrawHeader("Next Scene");
        Draw("m_nextLevel", true);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawHeader(string title)
    {
        UnityEditor.EditorGUILayout.LabelField(title, UnityEditor.EditorStyles.boldLabel);
    }

    private void Draw(string propertyName, bool includeChildren = false)
    {
        UnityEditor.SerializedProperty property = serializedObject.FindProperty(propertyName);

        if (property != null)
        {
            UnityEditor.EditorGUILayout.PropertyField(property, includeChildren);
        }
    }
}
#endif
