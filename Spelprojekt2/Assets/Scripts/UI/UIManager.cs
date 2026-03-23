using UnityEngine;

using UnityEngine.UI;
using UnityEngine.EventSystems;
using Interaction.Dialogue;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using AudioKit.FMOD;
using UnityEngine.InputSystem;
using TMPro;

public enum UIState
{
    None,
    MainMenu,
    Settings,
    PauseMenu,
    Dialogue,
}

public enum KeyAction
{
    KM_Forward,
    KM_Left,
    KM_Right,
    KM_Back,
    KM_Pickup,
    KM_Jump,
    KM_Interaction,
    KM_PrimaryFilter,
    KM_SecondaryFilter,
    KM_Sprint,
    KM_CameraChangeShoulder,
    KM_CameraZoomIn,
    KM_CameraZoomOut,
    KM_CameraFreeLookToggle,
    KM_CameraFirstPerson,
    KM_UIUp,
    KM_UIDown,
    KM_UISelect,
    KM_UIAdvance,
    C_Movement,
    C_Pickup,
    C_Jump,
    C_Interaction,
    C_PrimaryFilter,
    C_SecondaryFilter,
    C_Sprint,
    C_Look,
    C_CameraChangeShoulder,
    C_CameraZoom,
    C_CameraFreeLookToggle,
    C_CameraFirstPerson,
    C_UIMovement,
    C_UISelect,
    C_UIAdvance,
    COUNT,
}

public class UIManager : MonoBehaviour
{
    private static UIManager I;

    [SerializeField]
    private UIState m_stateOnInitialization;

    [SerializeField]
    private LevelsData m_levelData;

    [SerializeField]
    private LevelsData m_MainMenuLevelData;

    [SerializeField]
    private LevelData m_creditsLevelData;

    [SerializeField]
    private Player m_playerPrefab;

    [SerializeField]
    private AudioCueSO m_onButtonHoverCue;

    [SerializeField]
    private Color m_buttonColor;

    [SerializeField]
    private Color m_selectedButtonColor;

    [SerializeField]
    private SensitivityData m_sensitivity;

    [SerializeField]
    private GameObject m_mainMenuParent;

    [SerializeField]
    private GameObject m_pauseMenuParent;

    [Header("Settings Page")]
    [SerializeField]
    private GameObject m_settingsMenuParent;

    [SerializeField]
    private Button[] m_primaryFilterButtons;

    [SerializeField]
    private Button[] m_secondaryFilterButtons;

    [SerializeField]
    private GameObject m_settingsPageAudio;

    [SerializeField]
    private GameObject m_settingsPageControls;

    [SerializeField]
    private GameObject m_settingsPageGameplay;

    [SerializeField]
    private Button m_settingsPageAudioButton;

    [SerializeField]
    private Button m_settingsPageControlsButton;

    [SerializeField]
    private Scrollbar m_settingsPageSensitivitySlider;

    [SerializeField]
    private Scrollbar m_settingsPage_MasterVolumeSlider;

    [SerializeField]
    private Scrollbar m_settingsPage_MusicVolumeSlider;

    [SerializeField]
    private Scrollbar m_settingsPage_SFXVolumeSlider;

    [SerializeField]
    private Scrollbar m_settingsPage_UIVolumeSlider;

    [SerializeField]
    private Button m_settingsPageGameplayButton;

    [Header("Controls")]

    [SerializeField]
    private Button m_controlsPageControllerButton;

    [SerializeField]
    private Button m_controlsPageKeyboardButton;

    [SerializeField]
    private GameObject m_controlsPageController;

    [SerializeField]
    private GameObject m_controlsPageKeyboard;

    [SerializeField]
    private EventSystem m_eventSystem;

    [SerializeField]
    private GameObject m_settingsSelected;

    [SerializeField]
    private GameObject m_firstMainMenuSelected;

    [SerializeField]
    private GameObject m_firstPauseMenuSelected;

    private UIState m_state;

    // TODO(ah): Do something stack based over this nonsense
    private UIState m_statePriorToSettingsMenu;
    
    private bool m_transitionFromMainMenuToGameIsDone;
    private SceneLoader m_sceneLoader;
    private LevelData m_loadedLevel;

    void Awake()
    {
        if(I != null && I != this)
        {
            Destroy(this.gameObject);
            return;
        }

        LevelManager.SetCurrentLevel(m_MainMenuLevelData.m_levels[0]);

        I = this;
        DontDestroyOnLoad(this.gameObject);

        InputManager.onRebindComplete.AddListener(CompleteRemap);
        PersistentDataManager.DeserializeSettings();

        // ah: init filter button listeners
        {
            for(int i = 0; i < m_primaryFilterButtons.Length; i++)
            {
                var btn = m_primaryFilterButtons[i];
                FilterColor color = (FilterColor)i;
                SetButtonColor(btn, m_buttonColor);
                btn.onClick.AddListener(() => PrimaryFilterButtonClicked(color));
            }

            for(int i = 0; i < m_secondaryFilterButtons.Length; i++)
            {
                var btn = m_secondaryFilterButtons[i];
                FilterColor color = (FilterColor)i;
                SetButtonColor(btn, m_buttonColor);
                btn.onClick.AddListener(() => SecondaryFilterButtonClicked(color));
            }

        }

        EnterState(m_stateOnInitialization);
    }

    void SecondaryFilterButtonClicked(FilterColor color)
    {
        FilterManager fm = Object.FindFirstObjectByType<FilterManager>();
        FilterColor[] colors = fm.m_filterColorData.m_Colors;

        var primary   = colors[0];
        var secondary = colors[1];

        if(secondary != color)
        {
            colors[1]            = color;

            if(primary == color)
            {
                colors[0] = (FilterColor)(((int)primary + 1) % (int)FilterColor.COUNT);
                SetButtonColor(m_primaryFilterButtons[(int)primary], m_buttonColor);
            }

            SetButtonColor(m_secondaryFilterButtons[(int)secondary], m_buttonColor);
            SetButtonColor(m_primaryFilterButtons[(int)colors[0]], m_selectedButtonColor);
            SetButtonColor(m_secondaryFilterButtons[(int)colors[1]], m_selectedButtonColor);

            fm.ChangeFilterColor();
        }
    }

    void PrimaryFilterButtonClicked(FilterColor color)
    {
        FilterManager fm = Object.FindFirstObjectByType<FilterManager>();
        FilterColor[] colors = fm.m_filterColorData.m_Colors;
        var primary   = colors[0];
        var secondary = colors[1];

        if(primary != color)
        {
            colors[0]            = color;

            if(secondary == color)
            {
                colors[1] = (FilterColor)(((int)secondary + 1) % (int)FilterColor.COUNT);
                SetButtonColor(m_secondaryFilterButtons[(int)secondary], m_buttonColor);
            }

            SetButtonColor(m_primaryFilterButtons[(int)primary], m_buttonColor);
            SetButtonColor(m_primaryFilterButtons[(int)colors[0]], m_selectedButtonColor);
            SetButtonColor(m_secondaryFilterButtons[(int)colors[1]], m_selectedButtonColor);

            fm.ChangeFilterColor();
        }
    }

    public void SetSelectGameObject(GameObject obj)
    {
        I.m_eventSystem.SetSelectedGameObject(obj);
    }

    public void CompleteRemap(KeyAction action)
    {
        var rebindButtons = FindObjectsByType<RebindButton>(FindObjectsSortMode.None);
        foreach(var btn in rebindButtons)
        {
            btn.UpdateText();
        }
    }

    public void PlayHoverButtonSound()
    {
        SfxDirector.PlayCue2(m_onButtonHoverCue, Vector3.zero);
    }

    public void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ShowCursor()
    {
        Cursor.visible   = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public static void EnterState(UIState state)
    {
        if(!I)
        {
            return;
        }
        switch(state)
        {
            case UIState.Settings:
            {
                I.ShowCursor();
                InputManager.DisablePlayerInput();
                I.m_settingsMenuParent.SetActive(true);
                I.m_statePriorToSettingsMenu = I.m_state;

                I.m_eventSystem.SetSelectedGameObject(I.m_settingsSelected);
                break;
            }
            case UIState.None:
            {
                I.HideCursor();
                InputManager.EnablePlayerInput();
                break;
            }
            case UIState.MainMenu:
            {
                I.ShowCursor();
                I.m_mainMenuParent.SetActive(true);
                InputManager.DisablePlayerInput();
                I.m_eventSystem.SetSelectedGameObject(I.m_firstMainMenuSelected);
                break;
            }
            case UIState.PauseMenu:
            {
                I.ShowCursor();
                I.m_pauseMenuParent.SetActive(true);
                InputManager.DisablePlayerInput();
                I.m_eventSystem.SetSelectedGameObject(I.m_firstPauseMenuSelected);
                break;
            }
            case UIState.Dialogue:
            {
                I.ShowCursor();
                InputManager.DisablePlayerInput();
                break;
            }
        }
        I.m_state   = state;
    }

    private enum SettingsPageKind
    {
        Controls,
        Gameplay,
        Audio,
    }
    private enum ControlsPageKind
    {
        Keyboard,
        Controller,
    }
    private SettingsPageKind m_activeSettingsPage;
    private ControlsPageKind m_activeControlsPage;

    void Start()
    {
        // ah: Set default values in settings menu
        {
            // ah: current header

            // ah: current filters
            FilterManager fm = Object.FindFirstObjectByType<FilterManager>();
            if(fm != null)
            {
                FilterColor[] colors = fm.m_filterColorData.m_Colors;

                SetButtonColor(m_primaryFilterButtons[(int)colors[0]], m_selectedButtonColor);
                SetButtonColor(m_secondaryFilterButtons[(int)colors[1]], m_selectedButtonColor);
            }

            // ah: settings page
            SetActiveSettingsPage(m_activeSettingsPage);

            // ah: sensitivity
            m_settingsPageSensitivitySlider.value = (m_sensitivity.m_currentSensitivity - m_sensitivity.m_minSensitivity) / (m_sensitivity.m_maxSensitivity - m_sensitivity.m_minSensitivity);
        }
    }

    public void MainMenu_PlayClicked()
    {
        // Query PDM for which level to load
        LevelData level_to_load = PersistentDataManager.LevelToLoad(m_levelData);

        LevelManager.m_onTransitionEnd += SetupScene;
        LevelManager.TransitionToSceneAsync(level_to_load);
        
        m_mainMenuParent.SetActive(false);
        EnterState(UIState.None);
    }

    public void MainMenu_SettingsClicked()
    {
        m_mainMenuParent.SetActive(false);
        EnterState(UIState.Settings);
    }

    public void MainMenu_DeleteSaveClicked()
    {
        PersistentDataManager.DeleteSave();
        GlobalInkVariableManager.ClearAll();
        FilterManager.m_filterUnlocked = false;
    }

    public void MainMenu_CreditsClicked()
    {
        m_mainMenuParent.SetActive(false);
        EnterState(UIState.None);
        LevelManager.TransitionToSceneAsync(m_creditsLevelData, 0);
    }

    public void MainMenu_QuitClicked()
    {
        PersistentDataManager.SerializeSettings();
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #endif
        Application.Quit();
    }

    public void PauseMenu_ResumeClicked()
    {
        I.m_pauseMenuParent.SetActive(false);
        EnterState(UIState.None);
    }

    public void PauseMenu_SaveClicked()
    {
        PersistentDataManager.SerializeAll();
    }

    public void PauseMenu_GoToCheckpointClicked()
    {
        LevelCheckpointManager.Respawn();
    }

    public void PauseMenu_SettingsClicked()
    {
        I.m_pauseMenuParent.SetActive(false);
        EnterState(UIState.Settings);
    }

    public void PauseMenu_MainMenuClicked()
    {
        I.m_pauseMenuParent.SetActive(false);
        PersistentDataManager.SerializeAll();
        Player player = FindFirstObjectByType<Player>();
        Destroy(player.gameObject);

        // HACK(ah): just want no ui shown here, don't want to enter state
        this.m_state  = UIState.None;
        LevelManager.m_onTransitionEnd += SetupMainMenu;
        LevelManager.TransitionToSceneAsync(m_MainMenuLevelData.m_levels[0], 0.0f);
    }

    public void SettingsMenu_BackClicked()
    {
        I.m_settingsMenuParent.SetActive(false);
        EnterState(m_statePriorToSettingsMenu);
    }

    public void SettingsMenu_SensitivitySliderValueChange()
    {
        float s               = (m_sensitivity.m_currentSensitivity - m_sensitivity.m_minSensitivity) / (m_sensitivity.m_maxSensitivity - m_sensitivity.m_minSensitivity);
        float new_sensitivity = m_settingsPageSensitivitySlider.value;

        if(Mathf.Abs(s - new_sensitivity) > 0.00001f)
        {
            m_sensitivity.m_currentSensitivity = Mathf.Clamp(new_sensitivity * (m_sensitivity.m_maxSensitivity - m_sensitivity.m_minSensitivity), 
                    m_sensitivity.m_minSensitivity, m_sensitivity.m_maxSensitivity);
        }
    }

    public void SettingsMenu_MasterAudioSetSliderValue()
    {
        m_settingsPage_MasterVolumeSlider.value = AudioSystem.I.GetMasterVolume();
    }

    public void SettingsMenu_MasterAudioValueChange()
    {
        AudioSystem.I.SetMasterVolume(m_settingsPage_MasterVolumeSlider.value);
    }

    public void SettingsMenu_MusicAudioSetSliderValue()
    {
        m_settingsPage_MusicVolumeSlider.value = AudioSystem.I.GetMusicVolume();
    }

    public void SettingsMenu_MusicAudioValueChange()
    {
        AudioSystem.I.SetMusicVolume(m_settingsPage_MusicVolumeSlider.value);
    }

    public void SettingsMenu_SFXAudioSetSliderValue()
    {
        m_settingsPage_SFXVolumeSlider.value = AudioSystem.I.GetSfxVolume();
    }

    public void SettingsMenu_SFXAudioValueChange()
    {
        AudioSystem.I.SetSfxVolume(m_settingsPage_SFXVolumeSlider.value);
    }

    public void SettingsMenu_UIAudioSetSliderValue()
    {
        m_settingsPage_UIVolumeSlider.value = AudioSystem.I.GetUiVolume();
    }

    public void SettingsMenu_UIAudioValueChange()
    {
        AudioSystem.I.SetUiVolume(m_settingsPage_UIVolumeSlider.value);
    }

    void SetButtonColor(Button btn, Color color)
    {
        var cb = btn.colors;
        cb.normalColor = cb.highlightedColor = cb.disabledColor = cb.pressedColor = cb.selectedColor = color;
        btn.colors = cb;
    }

    void SetActiveControlsPage(ControlsPageKind kind)
    {
        m_controlsPageKeyboard.SetActive(kind == ControlsPageKind.Keyboard);
        m_controlsPageController.SetActive(kind == ControlsPageKind.Controller);
        m_activeControlsPage = kind;
    }

    void SetActiveSettingsPage(SettingsPageKind kind)
    {
        m_activeSettingsPage = kind;
        m_settingsPageGameplay.SetActive(kind == SettingsPageKind.Gameplay);
        m_settingsPageAudio.SetActive(kind == SettingsPageKind.Audio);
        m_settingsPageControls.SetActive(kind == SettingsPageKind.Controls);
        switch(kind)
        {
            case SettingsPageKind.Audio:
            {
                SettingsMenu_MasterAudioSetSliderValue();
                SettingsMenu_MusicAudioSetSliderValue();
                SettingsMenu_SFXAudioSetSliderValue();
                SettingsMenu_UIAudioSetSliderValue();

            }break;
            case SettingsPageKind.Controls:
            {
                SetActiveControlsPage(m_activeControlsPage);
            }break;
        }
    }

    public void SettingsMenu_AudioClicked()
    {
        SetActiveSettingsPage(SettingsPageKind.Audio);
    }

    public void SettingsMenu_ControlsClicked()
    {
        SetActiveSettingsPage(SettingsPageKind.Controls);
    }

    public void SettingsMenu_GameplayClicked()
    {
        SetActiveSettingsPage(SettingsPageKind.Gameplay);
    }

    public void SettingsMenu_ControlsKeyboardClicked()
    {
        SetActiveControlsPage(ControlsPageKind.Keyboard);
    }

    public void SettingsMenu_ControlsControllerClicked()
    {
        SetActiveControlsPage(ControlsPageKind.Controller);
    }

    void SetupScene()
    {
        Player player = Instantiate(m_playerPrefab);

        PersistentDataManager.DeserializeLoadedScenes();
        var handler = Object.FindFirstObjectByType<PlayerTransformHandler>();
        var camera = Object.FindFirstObjectByType<PlayerCamera>();
        DeserializedPlayerResult ok = PersistentDataManager.DeserializePlayer(player, handler, camera);
        if(ok.found)
        {
            if(ok.active_filter != FilterKind.None)
            {
                FilterManager fm = Object.FindFirstObjectByType<FilterManager>();
                if(fm != null)
                {
                    fm.ChangeFilter(ok.active_filter);
                }
            }
        }
        else
        {
            LevelCheckpointManager.Respawn();
        }
        EnterState(UIState.None);
        LevelManager.m_onTransitionEnd -= SetupScene;
    }

    public static void SetupMainMenu()
    {
        EnterState(UIState.MainMenu);
        LevelManager.m_onTransitionEnd -= SetupMainMenu;
    }

    void Update()
    {

        if(m_state == UIState.None && InputManager.Paused())
        {
            EnterState(UIState.PauseMenu);
        }
        else if(m_state == UIState.PauseMenu && InputManager.Unpaused())
        {
            EnterState(UIState.None);
        }
        else if(m_state == UIState.Settings && InputManager.Unpaused())
        {
            I.m_settingsMenuParent.SetActive(false);
            I.m_settingsSelected = I.m_eventSystem.currentSelectedGameObject;
            EnterState(m_statePriorToSettingsMenu);
        }
    }
}
