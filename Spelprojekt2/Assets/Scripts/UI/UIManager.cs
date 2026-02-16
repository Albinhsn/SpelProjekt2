using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using AudioKit.FMOD;

public enum UIState
{
    None,
    MainMenu,
    Settings,
    PauseMenu,
}

[RequireComponent(typeof(GlitchTransitionManager))]
public class UIManager : MonoBehaviour
{
    private static UIManager m_instance;

    [SerializeField]
    private Font font;

    [SerializeField]
    private UIState m_stateOnInitialization;

    [SerializeField]
    private LevelsData m_levelData;

    [SerializeField]
    private LevelsData m_MainMenuLevelData;

    [SerializeField]
    private Player m_playerPrefab;

    [SerializeField]
    private float m_areaWidth;

    [SerializeField]
    private float m_areaHeight;

    [SerializeField]
    private float m_btnWidth;

    [SerializeField]
    private float m_btnHeight;

    [SerializeField]
    private float m_sliderWidth;

    [SerializeField]
    private Texture2D m_btnTexture;

    [SerializeField]
    private Texture2D m_pauseMenuBG;

    [SerializeField]
    private GlitchTransitionManager m_glitchTransitionManager;

    [SerializeField]
    private AudioCueSO m_onButtonHoverCue;

    private UIState m_state;

    // TODO(ah): Do something stack based over this nonsense
    private UIState m_statePriorToSettingsMenu;
    
    private AudioSystem m_audioSystem;
    private bool m_transitionFromMainMenuToGameIsDone;
    private SceneLoader m_sceneLoader;
    private LevelData m_loadedLevel;


    // (ah): Signal state
    struct UI_Signal
    {
        public bool m_hot;
        public bool m_active;
    }

    private Dictionary<string, UI_Signal> m_buttonSignals;
    private int m_selectedIndex = 0;
    private bool m_useSelected;

    private int m_cursorUsages = 0;
    public void HideCursor()
    {
        m_cursorUsages--;
        if (m_cursorUsages <= 0)
        {
            m_cursorUsages = 0;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void ShowCursor()
    {
        m_cursorUsages++;
        Cursor.visible   = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private int m_pauseOverrideCount = 0;
    public void AllowPause()
    {
        --m_pauseOverrideCount;
        if (m_pauseOverrideCount < 1)
        {
            m_pauseOverrideCount = 0;
        }
    }
    public void DisallowPause()
    {
        ++m_pauseOverrideCount;
    }


    private void EnterState(UIState state)
    {
        switch(state)
        {
            case UIState.Settings:
            {
                ShowCursor();
                InputManager.DisablePlayerInput();
                m_statePriorToSettingsMenu = m_state;
                break;
            }
            case UIState.None:
            {
                HideCursor();
                InputManager.EnablePlayerInput();
                break;
            }
            case UIState.MainMenu:
            {
                ShowCursor();
                InputManager.DisablePlayerInput();
                break;
            }
            case UIState.PauseMenu:
            {
                ShowCursor();
                InputManager.DisablePlayerInput();
                break;
            }
        }

        m_selectedIndex = 0;
        this.m_state   = state;
    }

    void Awake()
    {

        if(m_instance != null && m_instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        LevelManager.SetCurrentLevel(m_MainMenuLevelData.m_levels[0]);

        m_instance = this;
        m_buttonSignals = new();
        DontDestroyOnLoad(this.gameObject);

        EnterState(m_stateOnInitialization);
    }

    void Start()
    {
        m_audioSystem = FindFirstObjectByType<AudioSystem>();
    }


    bool MenuBtn(string text, int index)
    {

        UI_Signal signal = new();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUILayoutOption[] button_options = new GUILayoutOption[2];
        button_options[0] = GUILayout.Width(m_btnWidth);
        button_options[1] = GUILayout.Height(m_btnHeight);

        UI_Signal prev_signal = new();
        m_buttonSignals.TryGetValue(text, out prev_signal);

        GUIStyle btn_style = new GUIStyle(GUI.skin.button);
        btn_style.normal.background = m_selectedIndex == index ? btn_style.onHover.background : btn_style.normal.background;
        btn_style.hover.background  = m_selectedIndex == index ? btn_style.onHover.background : btn_style.normal.background;
        btn_style.font = font;

        bool result = GUILayout.Button(text, btn_style, button_options) || (index == m_selectedIndex && m_useSelected);

        Rect btn_rect = GUILayoutUtility.GetLastRect();

        // HACK(ah): God i fucking hate unity with a passion
        if(Event.current.type == EventType.Repaint)
        {
            // Create signal
            signal.m_active = result;
            signal.m_hot    = btn_rect.Contains(Event.current.mousePosition);

            if(m_buttonSignals.ContainsKey(text))
            {
                if(!prev_signal.m_hot && signal.m_hot)
                {
                    SfxDirector.PlayCue2(m_onButtonHoverCue, Vector3.zero);
                    m_selectedIndex = index;
                }
            }
            else if(signal.m_hot)
            {
                SfxDirector.PlayCue2(m_onButtonHoverCue, Vector3.zero);
            }
            m_buttonSignals[text] = signal;
        }


        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        return result;
    }


    float
    Slider(float initial_value)
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUILayoutOption[] options = new GUILayoutOption[1];
        options[0] = GUILayout.Width(m_sliderWidth);

        GUIStyle slider_style = new GUIStyle(GUI.skin.horizontalSlider);
        slider_style.padding.top = -2;

        float result = GUILayout.HorizontalSlider(initial_value, 0, 1, slider_style, GUI.skin.horizontalScrollbarThumb, options);

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        return result;
    }

    void
    AreaBegin(float w, float h)
    {
        GUIStyle window_style    = new GUIStyle(GUI.skin.window);
        window_style.padding.top = 0;

        float x = (Screen.width - w) * 0.5f;
        float y = (Screen.height - h) * 0.5f;

        Rect window_rect         = new Rect(x, y, w, h);
        GUI.Box(window_rect, GUIContent.none);

        GUILayout.BeginArea(window_rect);
        GUILayout.BeginVertical();

        GUILayout.FlexibleSpace();

    }

    void 
    AreaEnd()
    {
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    int 
    MenuSelection(int prev_value, string label, string[] selections)
    {
        GUIStyle style  = new(GUI.skin.button);
        style.fontSize -= 10;
        style.font      = font;

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUIStyle label_style = new GUIStyle(GUI.skin.label);
        label_style.font = font;
        label_style.fontSize = 15;
        GUILayout.Label(label, label_style);

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        int new_value = GUILayout.SelectionGrid(prev_value, selections, (int)FilterColor.COUNT, style);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        return new_value;

    }

    void SetupScene()
    {
        Player player = Instantiate(m_playerPrefab);

        // Deserialize player
        DeserializedPlayerResult ok = PersistentDataManager.DeserializePlayer(player);
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
            // Find first checkpoint of the loaded level
        }
        EnterState(UIState.None);
        LevelManager.m_onTransitionEnd -= SetupScene;
    }

    void OnGUI()
    {
        int btn_index = 0;
        switch(m_state)
        {

            case UIState.MainMenu:
            {
                AreaBegin(m_areaWidth, m_areaHeight);

                if(MenuBtn("Play", btn_index++))
                {
                    // Query PDM for which level to load
                    LevelData level_to_load = PersistentDataManager.LevelToLoad(m_levelData);

                    LevelManager.m_onTransitionEnd += SetupScene;
                    LevelManager.TransitionToSceneAsync(level_to_load);
                    
                    EnterState(UIState.None);
                }

                GUILayout.Space(25);
                if(MenuBtn("Settings", btn_index++))
                {
                    EnterState(UIState.Settings);
                }


                if(MenuBtn("Delete save", btn_index++))
                {
                    PersistentDataManager.RemoveAllSerializedData();
                }

                GUILayout.Space(25);
                if(MenuBtn("Quit", btn_index++))
                {
                    PersistentDataManager.RemoveAllSerializedData();
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #endif
                    Application.Quit();
                }

                AreaEnd();
                break;
            }
            case UIState.Settings:
            {
                AreaBegin(m_areaWidth * 2.0f, m_areaHeight * 1.25f);

                // Volume slider
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    GUILayout.Label("Volume");

                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    if(m_audioSystem != null)
                    {
                        float audio_volume = m_audioSystem.GetMasterVolume();
                        float new_volume   = Slider(audio_volume);
                        if(new_volume != audio_volume)
                        {
                            m_audioSystem.SetMasterVolume(new_volume);
                        }
                    }
                }

                // Color Accessibility
                {

                    FilterManager fm = FindFirstObjectByType<FilterManager>();
                    if(fm != null)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        GUILayout.Label("Color settings");

                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();


                        FilterColor[] colors = fm.m_filterColorData.m_Colors;

                        int prev_primary   = (int)colors[0];
                        int prev_secondary = (int)colors[1];

                        string[] filter_color_strings = new string[(int)FilterColor.COUNT];
                        for(int i = 0; i < (int)FilterColor.COUNT; i++)
                        {
                            filter_color_strings[i] = ((FilterColor)i).ToString();
                        }

                        bool change_filter = false;
                        int new_primary = MenuSelection(prev_primary, "Primary", filter_color_strings);
                        if(prev_primary != new_primary)
                        {
                            if(new_primary == prev_secondary)
                            {
                                prev_secondary = (new_primary + 1) % (int)FilterColor.COUNT;
                            }
                            change_filter = true;
                        }

                        int new_secondary = MenuSelection(prev_secondary, "Secondary", filter_color_strings);
                        if(prev_secondary != new_secondary)
                        {
                            if(new_secondary == new_primary)
                            {
                                new_primary = (new_secondary + 1) % (int)FilterColor.COUNT;
                            }
                            change_filter = true;
                        }

                        if(change_filter)
                        {
                            colors[0] = (FilterColor)new_primary;
                            colors[1] = (FilterColor)new_secondary;
                            fm.ChangeFilterColor();
                        }
                    }
                }

                GUILayout.Space(15);

                if(MenuBtn("Back", btn_index++))
                {
                    EnterState(m_statePriorToSettingsMenu);
                }

                AreaEnd();
                break;
            }
            case UIState.PauseMenu:
            {
                AreaBegin(m_areaWidth, m_areaHeight);

                if(MenuBtn("Resume", btn_index++))
                {
                    EnterState(UIState.None);
                }

                if(MenuBtn("Save", btn_index++))
                {
                    PersistentDataManager.SerializeLoadedLevels();

                    Player player = FindFirstObjectByType<Player>();
                    if(player != null)
                    {
                        PersistentDataManager.SerializePlayer(player);
                    }
                    else
                    {
                        Debug.LogWarning("Tried to save without any player?");
                    }
                }

                if(MenuBtn("Reset to checkpoint", btn_index++))
                {
                    LevelCheckpointManager.ResetToCheckpoint();
                }

                if(MenuBtn("Settings", btn_index++))
                {
                    EnterState(UIState.Settings);
                }

                if(MenuBtn("Main Menu", btn_index++))
                {
                    PersistentDataManager.SerializeLoadedLevels();
                    Player player = FindFirstObjectByType<Player>();
                    if(player != null)
                    {
                        PersistentDataManager.SerializePlayer(player);
                        Destroy(player.gameObject);
                    }
                    else
                    {
                        Debug.LogWarning("Tried to save without any player?");
                    }
                    EnterState(UIState.MainMenu);

                    LevelManager.TransitionToScene(m_MainMenuLevelData.m_levels[0]);
                }

                AreaEnd();
                break;
            }
            default:{ break;}
        }

        // HACK(ah): (:
        if(Event.current.type == EventType.KeyUp && btn_index > 0)
        {
            if(Input.GetKeyUp(KeyCode.UpArrow))
            {
                m_selectedIndex--;
                if(m_selectedIndex < 0)
                {
                    m_selectedIndex = btn_index - 1;
                }
                SfxDirector.PlayCue2(m_onButtonHoverCue, Vector3.zero);
            }
            if(Input.GetKeyUp(KeyCode.DownArrow))
            {
                m_selectedIndex++;
                if(m_selectedIndex >= btn_index)
                {
                    m_selectedIndex = 0;
                }
                SfxDirector.PlayCue2(m_onButtonHoverCue, Vector3.zero);
            }
        }

        m_useSelected = false;
    }

    void Update()
    {
        if(Input.GetKeyUp(KeyCode.Return))
        {
            m_useSelected = true;
        }
        

        if(m_state == UIState.None)
        {
            if(Input.GetKeyUp(KeyCode.Escape) && m_pauseOverrideCount == 0)
            {
                EnterState(UIState.PauseMenu);
            }
        }
    }


}
