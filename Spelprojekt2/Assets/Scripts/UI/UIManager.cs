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

    private bool m_firstPauseMenuFrame;

    private UIState m_state;

    // TODO(ah): Do something stack based over this nonsense
    private UIState m_statePriorToSettingsMenu;
    private UIState m_statePriorToConsole;
    
    private AudioSystem m_audioSystem;

    void Awake()
    {

        if(m_instance != null && m_instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        m_instance = this;
        DontDestroyOnLoad(this.gameObject);

        m_state = m_stateOnInitialization;
    }

    void Start()
    {
        m_audioSystem = FindFirstObjectByType<AudioSystem>();
    }

    bool MenuBtn(string text)
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUILayoutOption[] button_options = new GUILayoutOption[2];
        button_options[0] = GUILayout.Width(m_btnWidth);
        button_options[1] = GUILayout.Height(m_btnHeight);

        GUIStyle btn_style = new GUIStyle(GUI.skin.button);
        btn_style.normal.background = m_btnTexture;
        btn_style.font = font;

        bool result = GUILayout.Button(text, btn_style, button_options);

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

    void StartGame()
    {
        InputManager.EnablePlayerInput();
        if(m_levelData != null)
        {
            LevelData data = m_levelData.levels[0];
            SceneLoader loader = new(data.m_sceneName, data.m_offset, m_playerPrefab);
            loader.Load();

            // TODO(ah): stream in different levels depending on where you are

            m_state = UIState.None;

            SceneManager.UnloadSceneAsync("MainMenu");
        }
    }

    void OnGUI()
    {
        switch(m_state)
        {
            case UIState.MainMenu:
            {
                AreaBegin(m_areaWidth, m_areaHeight);

                if(MenuBtn("Play"))
                {
                    if(m_glitchTransitionManager != null)
                    {
                        m_glitchTransitionManager.m_onTransitionEnd.AddListener(StartGame);
                        m_glitchTransitionManager.StartTransition();
                        m_state = UIState.None;
                    }
                    else
                    {
                        StartGame();
                    }
                }

                GUILayout.Space(25);
                if(MenuBtn("Settings"))
                {
                    m_statePriorToSettingsMenu = UIState.MainMenu;
                    m_state = UIState.Settings;
                }


                if(MenuBtn("Delete save"))
                {
                    PersistentDataManager.RemoveAllSerializedData();
                }

                GUILayout.Space(25);
                if(MenuBtn("Quit"))
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

                if(MenuBtn("Back"))
                {
                    m_state = m_statePriorToSettingsMenu;
                }

                AreaEnd();
                break;
            }
            case UIState.PauseMenu:
            {
                AreaBegin(m_areaWidth, m_areaHeight);

                if(!m_firstPauseMenuFrame && Input.GetKeyUp(KeyCode.Escape))
                {
                    InputManager.EnablePlayerInput();
                    m_state = UIState.None;
                }

                if(MenuBtn("Resume"))
                {
                    InputManager.EnablePlayerInput();
                    m_state = UIState.None;
                }

                if(MenuBtn("Save"))
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

                if(MenuBtn("Reset to checkpoint"))
                {
                    LevelCheckpointManager.ResetToCheckpoint();
                }

                if(MenuBtn("Settings"))
                {
                    m_statePriorToSettingsMenu = UIState.PauseMenu;
                    m_state = UIState.Settings;
                }

                if(MenuBtn("Main Menu"))
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

                    m_state = UIState.MainMenu;
                    
                    Scene scene = SceneManager.GetActiveScene();
                    SceneManager.UnloadSceneAsync(scene.name);
                    // TODO(ah): Use the scene loader
                    SceneManager.LoadScene("MainMenu", LoadSceneMode.Additive);
                }

                m_firstPauseMenuFrame = false;
                AreaEnd();
                break;
            }
            default:{ break;}
        }

    }

    void Update()
    {

        if(m_state == UIState.None)
        {
            if(Input.GetKeyUp(KeyCode.Escape))
            {
                m_firstPauseMenuFrame = true;
                m_state = UIState.PauseMenu;
                InputManager.DisablePlayerInput();
            }
        }
    }


}
