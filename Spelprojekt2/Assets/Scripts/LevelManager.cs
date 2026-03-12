using System;
using AudioKit.FMOD;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

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

    LevelManager()
    {
        m_glitchAudio = Resources.Load<AudioCueSO>("Audio/AC_Glitch");
        m_duckSnapshot = "MusicDuck";
    }

    private LevelData m_currentLevel;
    private LevelData m_nextLevel;

    private AudioCueSO m_glitchAudio;
    private string m_duckSnapshot;
    private EventInstance m_soundInstance;

    private SceneLoader m_sceneLoader;

    private Player m_player;


    private bool m_isTransitioning;
    private bool m_isGlitchingDone;
    private bool m_scenesLoaded;
    private bool m_scenesUnloaded;
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

    // NOTE(ah): This is just for initialization
    public static void SetCurrentLevel(LevelData levelData)
    {
        Instance.m_currentLevel = levelData;
    }

    public static void TransitionToSceneAsync(LevelData levelData, float transition_time = 4.0f)
    {
        if(Instance.m_isTransitioning)
        {
            Debug.LogWarning("Already transitioning to a new level, cannot transition to another one until the current transition is finished");
            return;
        }

        // Set duck snapshot
        {
            var I = AudioSystem.I;
            if(I != null)
            {
                I.SetSnapshot(Instance.m_duckSnapshot, true, 0);
            }
        }

        // ah: Apply end actions
        AudioSceneSettings audio = UnityEngine.Object.FindFirstObjectByType<AudioSceneSettings>();
        if(audio != null)
        {
            audio.ApplyEndActions();
        }

        Instance.m_isGlitchingDone = false;
        Instance.m_scenesLoaded = false;
        Instance.m_nextLevel = levelData;
        Instance.m_isTransitioning = true;

        var gtm = UnityEngine.Object.FindFirstObjectByType<GlitchTransitionManager>();
        if(gtm == null)
        {
            SetGlitchBoolTrue();
        }
        else if(transition_time > 0)
        {
            GlitchTransitionManager.m_onTransitionEnd.AddListener(SetGlitchBoolTrue);
            if(!GlitchTransitionManager.StartTransition(transition_time, 0.0f))
            {
                Debug.LogError("[LevelManager] A transition is already happening when we're trying to start a new one");
            }
        }
        else
        {
            Instance.m_isGlitchingDone = true;
        }

        // ah: unload current scene async
        if(Instance.m_currentLevel.m_scene != null)
        {
            Instance.m_sceneLoader = new(Instance.m_currentLevel);
            Instance.m_sceneLoader.m_onAllScenesLoaded += SetScenesUnloadedBoolTrue;
            Instance.m_sceneLoader.UnloadAsync();        
            Debug.Log($"Unloaded scene {Instance.m_currentLevel.m_sceneName}");
        }
        else
        {
            SetScenesUnloadedBoolTrue();
        }


        // ah: set the player inactive during transitions
        {
            Instance.m_player = UnityEngine.Object.FindFirstObjectByType<Player>();
            if(Instance.m_player != null)
            {
                LevelCheckpointManager.m_allowChangeCheckpoint = false;
            }
        }

        // ah: play transition sound effect
        {
            Instance.m_soundInstance = RuntimeManager.CreateInstance(Instance.m_glitchAudio.evt);
            FMOD.RESULT ok = Instance.m_soundInstance.start();

            if(ok != FMOD.RESULT.OK)
            {
                Debug.Log($"Failed to start glitch sound in transition because {ok}");
            }
        }
    }

    public static void FinishTransition()
    {

        // ah: set the player active again
        {
            LevelCheckpointManager.m_allowChangeCheckpoint = true;
            if(Instance.m_player != null)
            {
                LevelCheckpointManager.SetFirstSpawnPoint();
                LevelCheckpointManager.Respawn(Instance.m_player);
                Instance.m_player.gameObject.SetActive(true);
            }
        }


        Instance.m_isGlitchingDone = false;
        Instance.m_scenesLoaded = false;
        Instance.m_currentLevel = Instance.m_nextLevel;
        Instance.m_isTransitioning = false;
        Instance.m_onTransitionEnd_?.Invoke();

        // ah: Change music
        AudioSceneSettings audio = UnityEngine.Object.FindFirstObjectByType<AudioSceneSettings>();
        if(audio != null)
        {
            audio.ApplyStartActions();
        }

        // Set duck snapshot
        {
            var I = AudioSystem.I;
            if(I != null)
            {
                I.SetSnapshot(Instance.m_duckSnapshot, false, 0);
            }
        }

        // ah: Change sky
        SkySettings sky = UnityEngine.Object.FindFirstObjectByType<SkySettings>();
        if(sky != null)
        {
            sky.Apply();
        }

        // ah: stop play transition sound effect
        {
            FMOD.RESULT ok = Instance.m_soundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            if(ok != FMOD.RESULT.OK)
            {
                Debug.Log($"Failed to stop glitch sound in transition because {ok}");
            }
            ok = Instance.m_soundInstance.release();
            if(ok != FMOD.RESULT.OK)
            {
                Debug.Log($"Failed to release glitch sound in transition because {ok}");
            }
        }

        // ah: enable credits
        {
            EndCreditsManager credits = UnityEngine.Object.FindFirstObjectByType<EndCreditsManager>();
            if(credits != null)
            {
                credits.StartCredits();
            }


        }
    }

    static void SetGlitchBoolTrue()
    {
        Instance.m_isGlitchingDone = true;
        if(Instance.m_scenesLoaded && Instance.m_scenesUnloaded)
        {
            FinishTransition();
        }
        GlitchTransitionManager.m_onTransitionEnd.RemoveListener(SetGlitchBoolTrue);
    }

    static void SetScenesUnloadedBoolTrue()
    {
        if(Instance.m_player != null)
        {
            Instance.m_player.gameObject.SetActive(false);
        }
        Instance.m_sceneLoader    = new(Instance.m_nextLevel);
        Instance.m_sceneLoader.m_onAllScenesLoaded += SetScenesLoadedBoolTrue;
        Instance.m_sceneLoader.LoadAsync();
        Instance.m_scenesUnloaded = true;
    }

    static void SetScenesLoadedBoolTrue()
    {
        Instance.m_scenesLoaded = true;
        if(Instance.m_isGlitchingDone && Instance.m_scenesUnloaded)
        {
            FinishTransition();
        }
    }
}
