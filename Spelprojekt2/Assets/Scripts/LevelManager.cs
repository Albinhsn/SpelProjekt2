using System;
using UnityEngine;

public sealed class LevelManager
{
    private static LevelManager _instance;
    private static LevelManager Instance {
    get
        {
            if(_instance == null)
            {
                _instance = new LevelManager();
            }
            return _instance;
        } 
    }
    private LevelData m_currentLevel;
    private LevelData m_nextLevel;
    private GlitchTransitionManager m_glitchTransitionManager;

    private bool m_isTransitioning;
    private bool m_isGlitchingDone;
    private bool m_scenesLoaded;
    private Action m_onTransitionEnd_;
    public static event Action m_onTransitionEnd
    {
        add
        {
            Instance.m_onTransitionEnd_ += value;
        }
        remove
        {
            Instance.m_onTransitionEnd_ -= value;
        }
    }

    public static void TransitionToScene(LevelData levelData)
    {
        if(Instance.m_isTransitioning)
        {
            Debug.LogWarning("Already transitioning to a new level, cannot transition to another one until the current transition is finished");
            return;
        }
        Instance.m_nextLevel = levelData;
        Instance.m_isTransitioning = true;
        SceneLoader sceneLoader = new(levelData);
        sceneLoader.Load();
        FinishTransition();
    }
    public static void TransitionToSceneAsync(LevelData levelData, LevelData currentLevel)
    {
        if(Instance.m_isTransitioning)
        {
            Debug.LogWarning("Already transitioning to a new level, cannot transition to another one until the current transition is finished");
            return;
        }
        Instance.m_isGlitchingDone = false;
        Instance.m_scenesLoaded = false;
        Instance.m_nextLevel = levelData;
        Instance.m_isTransitioning = true;
        Instance.m_currentLevel = currentLevel;
        SceneLoader sceneLoader = new(levelData);
        sceneLoader.m_onAllScenesLoaded += SetScenesLoadedBoolTrue;
        Instance.m_glitchTransitionManager = UnityEngine.Object.FindFirstObjectByType<GlitchTransitionManager>();
        if(Instance.m_glitchTransitionManager == null)
        {
            SetGlitchBoolTrue();
        }
        else
        {
            Instance.m_glitchTransitionManager.m_onTransitionEnd.AddListener(SetGlitchBoolTrue);
            Instance.m_glitchTransitionManager.StartTransition();
        }
        sceneLoader.LoadAsync();
    }

    public static void FinishTransition()
    {
        Debug.Log("Finishing transition");
        if(Instance.m_currentLevel.m_scene != null)
        {
            SceneLoader sceneLoader = new(Instance.m_currentLevel);
            sceneLoader.Unload();        
            Debug.Log($"Unloaded scene {Instance.m_currentLevel.m_sceneName}");
        }
        Instance.m_isGlitchingDone = false;
        Instance.m_scenesLoaded = false;
        Instance.m_currentLevel = Instance.m_nextLevel;
        Instance.m_isTransitioning = false;
        Instance.m_onTransitionEnd_?.Invoke();
    }

    static void SetGlitchBoolTrue()
    {
        Instance.m_isGlitchingDone = true;
        if(Instance.m_scenesLoaded)
        {
            FinishTransition();
        }
    }

    static void SetScenesLoadedBoolTrue()
    {
        Instance.m_scenesLoaded = true;
        if(Instance.m_isGlitchingDone)
        {
            FinishTransition();
        }
    }
}
