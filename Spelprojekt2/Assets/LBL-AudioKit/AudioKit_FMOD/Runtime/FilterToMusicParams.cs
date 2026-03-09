using UnityEngine;
using AudioKit.FMOD;

public sealed class FilterToMusicParams : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private AudioParameterDriver driver;

    private FilterManager filterManager;

    [Header("AudioParamLibrary keys")]
    [SerializeField] private string redKey = "music_red";
    [SerializeField] private string blueKey = "music_blue";

    [Header("Values")]
    [SerializeField] private float onValue = 1f;
    [SerializeField] private float offValue = 0f;

    [Header("Invert")]
    [Tooltip("Om FMOD är normalt (0=OFF,1=ON) och din logik känns baklänges, slå på detta.")]
    [SerializeField] private bool invertValueBeforeSend = true;

    [Header("Driver")]
    [SerializeField] private string sourceId = "filter";
    [SerializeField] private int priority = 200;

    [Header("Fade (seconds)")]
    [SerializeField] private float fadeOnSeconds = 0.35f;
    [SerializeField] private float fadeOffSeconds = 0.60f;

    private FilterKind redKind = FilterKind.Primary;
    private FilterKind blueKind = FilterKind.Secondary;

    private void Start()
    {
        filterManager = FindFirstObjectByType<FilterManager>();
        if (!driver) driver = FindFirstObjectByType<AudioParameterDriver>();
        ResolveKindsFromGameData();

        if (filterManager) filterManager.m_filterChanged.AddListener(OnFilterChanged);
        ApplyFromState();
    }

    private void OnDestroy()
    {
        if (filterManager) filterManager.m_filterChanged.RemoveListener(OnFilterChanged);
    }

    private void OnFilterChanged(FilterKind kind, bool activating)
    {
        ResolveKindsFromGameData();


        if (!activating)
        {
            Set(redKey, offValue, turningOn: false);
            Set(blueKey, offValue, turningOn: false);
            return;
        }

        if (kind == redKind)
        {
            Set(redKey, onValue, turningOn: true);
            Set(blueKey, offValue, turningOn: false);
        }
        else if (kind == blueKind)
        {
            Set(blueKey, onValue, turningOn: true);
            Set(redKey, offValue, turningOn: false);
        }
        else
        {
            Set(redKey, offValue, turningOn: false);
            Set(blueKey, offValue, turningOn: false);
        }

    }

    private void ApplyFromState()
    {
        ResolveKindsFromGameData();

        var active = FilterManager.m_activeFilter;

        if (active == redKind)
        {
            Set(redKey, onValue, turningOn: true);
            Set(blueKey, offValue, turningOn: false);
        }
        else if (active == blueKind)
        {
            Set(blueKey, onValue, turningOn: true);
            Set(redKey, offValue, turningOn: false);
        }
        else
        {
            Set(redKey, offValue, turningOn: false);
            Set(blueKey, offValue, turningOn: false);
        }
    }

    private void ResolveKindsFromGameData()
    {
        // fallback
        redKind = FilterKind.Primary;
        blueKind = FilterKind.Secondary;

        if (!filterManager) return;
        var data = filterManager.m_filterColorData;
        if (data == null || data.m_Colors == null) return;

        var colors = data.m_Colors;
        if (colors.Length <= (int)FilterKind.Secondary) return;

        var primary = colors[(int)FilterKind.Primary];
        var secondary = colors[(int)FilterKind.Secondary];

        if (primary == FilterColor.Red) redKind = FilterKind.Primary;
        if (secondary == FilterColor.Red) redKind = FilterKind.Secondary;

        if (primary == FilterColor.Blue) blueKind = FilterKind.Primary;
        if (secondary == FilterColor.Blue) blueKind = FilterKind.Secondary;
    }

    private void Set(string key, float value, bool turningOn)
    {
        if (!driver) return;

        // Invertera värdet mellan offValue<->onValue (fungerar även om du byter nivåer)
        float sendValue = value;
        if (invertValueBeforeSend)
        {
            float t = Mathf.InverseLerp(offValue, onValue, value);     // 0..1
            sendValue = Mathf.Lerp(offValue, onValue, 1f - t);         // inverterad
        }

        float fade = turningOn ? fadeOnSeconds : fadeOffSeconds;

        // (sourceId, paramKeyOrName, value, active, priority, fadeSeconds)
        driver.SetSourceActive(sourceId, key, sendValue, true, priority, fade);
    }
}
