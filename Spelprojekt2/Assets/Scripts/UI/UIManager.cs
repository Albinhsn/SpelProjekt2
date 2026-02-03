using UnityEngine;
using UnityEngine.AddressableAssets;
using AudioKit.FMOD;

public enum UIState
{
    None,
    MainMenu,
    Settings,
    PauseMenu,
}

public class UIManager : MonoBehaviour
{

    [SerializeField]
    private UIState m_stateOnInitialization;

    [SerializeField]
    private LevelsData m_levelData;

    [SerializeField]
    private GameObject m_playerPrefab;

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

    private UIState m_state;
    private UIState m_statePriorToSettingsMenu;
    
    private AudioSystem m_audioSystem;

    void Awake()
    {
        m_state = m_stateOnInitialization;
        m_audioSystem = FindFirstObjectByType<AudioSystem>();
    }

    bool PauseMenuBtn(string text)
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUILayoutOption[] button_options = new GUILayoutOption[2];
        button_options[0] = GUILayout.Width(m_btnWidth);
        button_options[1] = GUILayout.Height(m_btnHeight);

        bool result = GUILayout.Button(text, button_options);

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
    AreaBegin()
    {
        GUIStyle window_style    = new GUIStyle(GUI.skin.window);
        window_style.padding.top = 0;

        float w = m_areaWidth;
        float h = m_areaHeight;

        float x = (Screen.width - w) * 0.5f;
        float y = (Screen.height - h) * 0.5f;

        Rect window_rect         = new Rect(x, y, w, h);
        GUI.Box(window_rect, GUIContent.none);

        GUILayout.BeginArea(window_rect);
        GUILayout.BeginVertical();

        GUILayout.Space(100);
    }

    void 
    AreaEnd()
    {
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    void OnGUI()
    {
        switch(m_state)
        {
            case UIState.MainMenu:
            {
                AreaBegin();

                if(PauseMenuBtn("Play"))
                {
                    if(m_levelData != null)
                    {
                        LevelData data = m_levelData.levels[0];
                        SceneLoader loader = new(data.m_sceneReference, data.m_offset);
                        loader.Load();
                        Instantiate(m_playerPrefab);

                        m_state = UIState.None;
                    }
                }

                GUILayout.Space(25);
                if(PauseMenuBtn("Settings"))
                {
                    m_statePriorToSettingsMenu = UIState.MainMenu;
                    m_state = UIState.Settings;
                }

                GUILayout.Space(25);
                if(PauseMenuBtn("Quit"))
                {
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
                AreaBegin();

                // Volume slider
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    GUILayout.Label("Volume");

                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    float audio_volume = m_audioSystem.GetMasterVolume();
                    float new_volume   = Slider(audio_volume);
                    if(new_volume != audio_volume)
                    {
                        m_audioSystem.SetMasterVolume(new_volume);
                    }
                }

                GUILayout.Space(40);

                if(PauseMenuBtn("Back"))
                {
                    m_state = m_statePriorToSettingsMenu;
                }

                AreaEnd();
                break;
            }
            case UIState.PauseMenu:
            {
                AreaBegin();

                if(PauseMenuBtn("Continue"))
                {
                    m_state = UIState.None;
                }
                GUILayout.Space(25);
                if(PauseMenuBtn("Settings"))
                {
                    m_statePriorToSettingsMenu = UIState.PauseMenu;
                    m_state = UIState.Settings;
                }

                GUILayout.Space(25);
                if(PauseMenuBtn("Main Menu"))
                {
                    // TODO(ah): How do we manage this?
                    m_state = UIState.MainMenu;
                }

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
                m_state = UIState.PauseMenu;
            }
        }
    }


}
