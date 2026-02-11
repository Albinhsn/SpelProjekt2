using Unity.Cinemachine;
using UnityEngine;

public sealed class InputManager
{
    private static InputManager _instance;
    private static InputManager Instance {
    get
        {
            if(_instance == null)
            {
                _instance = new InputManager();
            }
            return _instance;
        } 
    }

    private InputManager()
    {
        m_inputActions = new InputSystem_Actions();
        m_inputActions.Enable();
    }
    
    private InputSystem_Actions m_inputActions;

    public static Vector2 ReadMovementValue()
    {
        Vector2 movement = Instance.m_inputActions.Player.Move.ReadValue<Vector2>();
        return movement;
    }
    public static bool Jumped()
    {
        return Instance.m_inputActions.Player.Jump.WasPressedThisFrame();
    }

    public static bool PickedUpItem()
    {
        return Instance.m_inputActions.Player.Interact.WasPressedThisFrame();
    }
    public static bool Paused()
    {
        DisablePlayerInput();
        return Instance.m_inputActions.UI.Pause.WasPressedThisFrame();
    }
    public static bool Unpaused()
    {
        EnablePlayerInput();
        return Instance.m_inputActions.UI.Pause.WasPressedThisFrame();
    }
    public static void DisablePlayerInput()
    {
        Instance.m_inputActions.Player.Disable();

        //Temp while using cinemachine for camera control
        var camera = GameObject.FindFirstObjectByType<CinemachineInputAxisController>();
        if(camera != null)
        {
            camera.enabled = false;
        }
    }

    public static void EnablePlayerInput()
    {
        Instance.m_inputActions.Player.Enable();

        //Temp while using cinemachine for camera control
        var camera = GameObject.FindFirstObjectByType<CinemachineInputAxisController>();
        if(camera != null)
        {
            camera.enabled = true;
        }
    }

    public static bool Filter(FilterKind kind)
    {
        bool result = false;
        switch(kind)
        {
            case FilterKind.Primary:
            {
                result = Instance.m_inputActions.Player.FilterPrimary.WasPressedThisFrame();
                break;
            }
            case FilterKind.Secondary:
            {
                result = Instance.m_inputActions.Player.FilterSecondary.WasPressedThisFrame();
                break;
            }
        }
        return result;
    }
}
