using UnityEngine;
using UnityEngine.AddressableAssets;

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
    private AssetReference m_firstLevelReference;

    [SerializeField]
    private GameObject m_playerPrefab;

    [SerializeField]
    private float m_areaWidth;

    [SerializeField]
    private float m_areaHeight;

    // Btn texture
    
    // Pause menu bg


    private UIState m_state;
    private UIState m_statePriorToSettingsMenu;

    // NOTE(ah): We might not want to store the audio volume here
    // but instead query it from the AudioSystem (but that doesn't exist at time of writing)
    // - 3 feb
    private float m_audioVolume;

    void Awake()
    {
        m_state = m_stateOnInitialization;

        // TODO(ah): get the AudioSystem and query for the initial audio volume
    }

    bool PauseMenuBtn(string text)
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUILayoutOption[] button_width = new GUILayoutOption[1];
        button_width[0] = GUILayout.Width(100);

        bool result = GUILayout.Button(text, button_width);

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
        options[0] = GUILayout.Width(150);

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
                    SceneLoader loader = new(m_firstLevelReference, new());
                    loader.Load();
                    Instantiate(m_playerPrefab);

                    m_state = UIState.None;
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

                    float volume = Slider(m_audioVolume);
                    if(volume != m_audioVolume)
                    {
                        m_audioVolume = volume;
                        // TODO(ah): Actually set volume
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
