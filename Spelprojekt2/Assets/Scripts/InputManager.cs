using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public enum Dir
{
    Y,
    X
}
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
    private Vector3 m_aimDirectionForward = Vector3.zero;
    private Vector3 m_aimDirectionRight = Vector3.zero;

    public static void SetAimDirection(Vector3 forward, Vector3 Right)
    {
        Instance.m_aimDirectionForward = forward;
        Instance.m_aimDirectionRight = Right;
    }
    public static Vector3 GetForwardAimDir()
    {
        return Instance.m_aimDirectionForward;
    }

    public static Vector3 GetRightAimDir()
    {
        return Instance.m_aimDirectionRight;
    }

    public static Vector2 ReadMovementValue()
    {
        Vector2 movement = Instance.m_inputActions.Player.Move.ReadValue<Vector2>();
        return movement;
    }
    public static Vector2 ReadUINavigationValue()
    {
        return Instance.m_inputActions.UI.Navigate.ReadValue<Vector2>() + Instance.m_inputActions.UI.NavigateController.ReadValue<Vector2>();    
    }
    public static Vector2 ReadPointerPosition()
    {
        return Instance.m_inputActions.UI.Point.ReadValue<Vector2>();
    }
    public static bool UIPointerSelect()
    {
        return Instance.m_inputActions.UI.PointerSelect.WasPressedThisFrame();
    }
    public static Vector2 ReadLookValue()
    {
        return Instance.m_inputActions.Player.Look.ReadValue<Vector2>();
    }
    public static void MoveCursor(Vector2 cursorPos)
    {
        Vector2 newPos = cursorPos + new Vector2(0,-1) + new Vector2(500,500) * Time.deltaTime *ReadUINavigationValue();
        Mouse.current.WarpCursorPosition(newPos);
    }
    public static bool CameraZoomIn()
    {
        return Instance.m_inputActions.Player.CameraZoom.ReadValue<Vector2>().y == 1;
    }
    public static bool CameraZoomOut()
    {
        return Instance.m_inputActions.Player.CameraZoom.ReadValue<Vector2>().y == -1;
    }
    public static bool SelectUIOption()
    {
        return Instance.m_inputActions.UI.Select.WasPressedThisFrame() || Instance.m_inputActions.UI.Submit.WasPressedThisFrame();
    }
    public static bool UIAdvance()
    {
        return Instance.m_inputActions.UI.Advance.WasPressedThisFrame();
    }
    public static bool Jumped()
    {
        return Instance.m_inputActions.Player.Jump.WasPressedThisFrame();
    }
    public static bool Sprinting()
    {
        return Instance.m_inputActions.Player.Sprint.IsPressed();
    }
    public static bool PickedUpItem()
    {
        return Instance.m_inputActions.Player.Pickup.WasPressedThisFrame();
    }
    public static bool Interact()
    {
        return Instance.m_inputActions.Player.Interact.WasPressedThisFrame();
    }
    public static bool Paused()
    {
        // DisablePlayerInput();
        return Instance.m_inputActions.UI.Pause.WasPressedThisFrame();
    }
    public static bool Unpaused()
    {
        // EnablePlayerInput();
        return Instance.m_inputActions.UI.Pause.WasPressedThisFrame();
    }
    public static bool CameraFirstPerson()
    {
        return Instance.m_inputActions.Player.CameraFirstPerson.IsPressed();
    }
    public static bool CameraFreeLookTogglePressed()
    {
        return Instance.m_inputActions.Player.CameraFreeLookToggle.WasPressedThisFrame();
    }
    public static bool CameraFreeLookToggleRealesed()
    {
        return Instance.m_inputActions.Player.CameraFreeLookToggle.WasReleasedThisFrame();
    }
    public static bool CameraFreeLookToggleHeld()
    {
        return Instance.m_inputActions.Player.CameraFreeLookToggle.IsPressed();
    }
    public static bool CameraChangeShoulder()
    {
        return Instance.m_inputActions.Player.CameraChangeShoulder.WasPressedThisFrame();
    }
    public static void DisablePlayerInput()
    {
        Instance.m_inputActions.Player.Disable();
    }

    public static void DisablePlayerMovement()
    {
        Instance.m_inputActions.Player.Move.Disable();
    }

    public static void EnablePlayerInput()
    {
        Instance.m_inputActions.Player.Enable();
    }

    public static void EnablePlayerMovement()
    {
        Instance.m_inputActions.Player.Move.Enable();
    }

    public static bool FlipGravity()
    {
        return Instance.m_inputActions.Player.FlipGravity.WasPressedThisFrame();
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
