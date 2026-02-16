using UnityEngine;
using AudioKit.FMOD;

public sealed class FilterToMusicParams : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private FilterManager filterManager;
    [SerializeField] private AudioParameterDriver driver;

    [Header("AudioParamLibrary keys")]
    [SerializeField] private string redKey = "music_red";
    [SerializeField] private string blueKey = "music_blue";

    [Header("Values")]
    [SerializeField] private float onValue = 1f;
    [SerializeField] private float offValue = 0f;

    [Header("Driver")]
    [SerializeField] private string sourceId = "filter";
    [SerializeField] private int priority = 200;

    [Header("Fade (seconds)")]
    [SerializeField] private float fadeOnSeconds = 0.35f;
    [SerializeField] private float fadeOffSeconds = 0.60f;

    private void Awake()
    {
        if (!filterManager) filterManager = FindFirstObjectByType<FilterManager>();
        if (!driver) driver = FindFirstObjectByType<AudioParameterDriver>();
    }

    private void OnEnable()
    {
        if (filterManager) filterManager.m_filterChanged.AddListener(OnFilterChanged);
        ApplyFromState();
    }

    private void OnDisable()
    {
        if (filterManager) filterManager.m_filterChanged.RemoveListener(OnFilterChanged);
    }

    private void OnFilterChanged(FilterKind kind, bool activating)
    {
        // Stänger man av ett filter -> båda till 0 (fadeOff)
        if (!activating)
        {
            Set(redKey, offValue, turningOn: false);
            Set(blueKey, offValue, turningOn: false);
            return;
        }

        // Slår man på ett filter -> en blir 1 (fadeOn), andra blir 0 (fadeOff)
        if (kind == FilterKind.Primary) // röd (1)
        {
            Set(redKey, onValue, turningOn: true);
            Set(blueKey, offValue, turningOn: false);
        }
        else if (kind == FilterKind.Secondary) // blå (2)
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
        var active = FilterManager.m_activeFilter;

        if (active == FilterKind.Primary)
        {
            Set(redKey, onValue, turningOn: true);
            Set(blueKey, offValue, turningOn: false);
        }
        else if (active == FilterKind.Secondary)
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

    private void Set(string key, float value, bool turningOn)
    {
        if (!driver) return;

        float t = turningOn ? fadeOnSeconds : fadeOffSeconds;

        // (sourceId, paramKeyOrName, value, active, priority, fadeSeconds)
        driver.SetSourceActive(sourceId, key, value, true, priority, t);
    }
}
