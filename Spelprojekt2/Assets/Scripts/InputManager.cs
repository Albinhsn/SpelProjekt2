using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

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
        m_onRebindComplete = new();
    }
    
    private InputSystem_Actions m_inputActions;
    private Vector3 m_aimDirectionForward = Vector3.zero;
    private Vector3 m_aimDirectionRight = Vector3.zero;
    public static UnityEvent<KeyAction> onRebindComplete => Instance.m_onRebindComplete;
    private UnityEvent<KeyAction> m_onRebindComplete;

    private int m_currentRebindBindingIndex;
    private KeyAction m_currentRebindKeyAction;

    private InputActionRebindingExtensions.RebindingOperation m_rebindingOperation;

    public static void StartRemap(KeyAction action)
    {
        Instance.StartRemap_(action);
    }
    
    private void StartRemap_(KeyAction action)
    {
        InputAction input_action = null;
        int binding_index = 0;
        switch(action)
        {
            case KeyAction.KM_Forward:
            {
                input_action  = m_inputActions.Player.Move;
                binding_index = input_action.bindings.IndexOf(x => x.isPartOfComposite && x.name == "up");
            }break;
            case KeyAction.KM_Back:
            {
                input_action = m_inputActions.Player.Move;
                binding_index = input_action.bindings.IndexOf(x => x.isPartOfComposite && x.name == "down");
            }break;
            case KeyAction.KM_Left:
            {
                input_action = m_inputActions.Player.Move;
                binding_index = input_action.bindings.IndexOf(x => x.isPartOfComposite && x.name == "left");
            }break;
            case KeyAction.KM_Right:
            {
                input_action = m_inputActions.Player.Move;
                binding_index = input_action.bindings.IndexOf(x => x.isPartOfComposite && x.name == "right");
            }break;
            case KeyAction.KM_Pickup:
            {
                input_action = m_inputActions.Player.Pickup;
                binding_index = 0;
            }break;
            case KeyAction.KM_Interaction:
            {
                input_action = m_inputActions.Player.Interact;
                binding_index = 0;
            }break;
            case KeyAction.KM_PrimaryFilter:
            {
                input_action = m_inputActions.Player.FilterPrimary;
                binding_index = 0;
            }break;
            case KeyAction.KM_SecondaryFilter:
            {
                input_action = m_inputActions.Player.FilterSecondary;
                binding_index = 0;
            }break;
            case KeyAction.C_Movement:
            {
                input_action  = m_inputActions.Player.Move;
                binding_index = 0;
            }break;
            case KeyAction.C_Pickup:
            {
                input_action = m_inputActions.Player.Pickup;
                binding_index = 0;
            }break;
            case KeyAction.C_Interaction:
            {
                input_action  = m_inputActions.Player.Interact;
                binding_index = 1;
            }break;
            case KeyAction.C_PrimaryFilter:
            {
                input_action = m_inputActions.Player.FilterPrimary;
                binding_index = 1;
            }break;
            case KeyAction.C_SecondaryFilter:
            {
                input_action = m_inputActions.Player.FilterSecondary;
                binding_index = 1;
            }break;
        }

        // ah: action is unbounded
        if(binding_index != -1)
        {
            input_action.Disable();
            m_rebindingOperation = input_action.PerformInteractiveRebinding()
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(CompleteRebind);
            m_rebindingOperation.WithTargetBinding(binding_index);
            m_rebindingOperation.Start();
            m_currentRebindKeyAction    = action;
            m_currentRebindBindingIndex = binding_index;
        }
        else
        {
            Debug.LogError($"Couldn't rebind {action}");
        }
    }


    private InputBinding InputBindingFromKeyAction(KeyAction action)
    {
        InputBinding binding = new();
        InputAction input_action = new();
        int binding_index = -1;
        switch(action)
        {
            case KeyAction.KM_Forward:
            {
                input_action  = Instance.m_inputActions.Player.Move;
                binding_index = input_action.bindings.IndexOf(x => x.isPartOfComposite && x.name == "up");
            }break;
            case KeyAction.KM_Back:
            {
                input_action  = Instance.m_inputActions.Player.Move;
                binding_index = input_action.bindings.IndexOf(x => x.isPartOfComposite && x.name == "down");
            }break;
            case KeyAction.KM_Left:
            {
                input_action  = Instance.m_inputActions.Player.Move;
                binding_index = input_action.bindings.IndexOf(x => x.isPartOfComposite && x.name == "left");
            }break;
            case KeyAction.KM_Right:
            {
                input_action  = Instance.m_inputActions.Player.Move;
                binding_index = input_action.bindings.IndexOf(x => x.isPartOfComposite && x.name == "right");
            }break;
            case KeyAction.KM_Pickup:
            {
                input_action = Instance.m_inputActions.Player.Pickup;
                binding_index = 0;
            }break;
            case KeyAction.KM_Interaction:
            {
                input_action = Instance.m_inputActions.Player.Interact;
                binding_index = 0;
            }break;
            case KeyAction.KM_PrimaryFilter:
            {
                input_action = Instance.m_inputActions.Player.FilterPrimary;
                binding_index = 0;
            }break;
            case KeyAction.KM_SecondaryFilter:
            {
                input_action = Instance.m_inputActions.Player.FilterSecondary;
                binding_index = 0;
            }break;
        }

        if(binding_index >= 0 && binding_index < input_action.bindings.Count)
        {
            binding = input_action.bindings[binding_index];
        }

        return binding;
    }
    
    void Unbind(KeyAction action)
    {
        switch(action)
        {
            case KeyAction.KM_Forward:
            {
                var input_action  = Instance.m_inputActions.Player.Move;
                input_action.ApplyBindingOverride(input_action.bindings.IndexOf(x => x.isPartOfComposite && x.name == "up"), "");
            }break;
            case KeyAction.KM_Back:
            {
                var input_action  = Instance.m_inputActions.Player.Move;
                input_action.ApplyBindingOverride(input_action.bindings.IndexOf(x => x.isPartOfComposite && x.name == "down"), "");
            }break;
            case KeyAction.KM_Left:
            {
                var input_action  = Instance.m_inputActions.Player.Move;
                input_action.ApplyBindingOverride(input_action.bindings.IndexOf(x => x.isPartOfComposite && x.name == "left"), "");
            }break;
            case KeyAction.KM_Right:
            {
                var input_action  = Instance.m_inputActions.Player.Move;
                input_action.ApplyBindingOverride(input_action.bindings.IndexOf(x => x.isPartOfComposite && x.name == "right"), "");
            }break;
            case KeyAction.KM_Pickup:
            {
                var input_action = Instance.m_inputActions.Player.Pickup;
                input_action.ApplyBindingOverride(0, "");
            }break;
            case KeyAction.KM_Interaction:
            {
                var input_action = Instance.m_inputActions.Player.Interact;
                input_action.ApplyBindingOverride(0, "");
            }break;
            case KeyAction.KM_PrimaryFilter:
            {
                var input_action = Instance.m_inputActions.Player.FilterPrimary;
                input_action.ApplyBindingOverride(0, "");
            }break;
            case KeyAction.KM_SecondaryFilter:
            {
                var input_action = Instance.m_inputActions.Player.FilterSecondary;
                input_action.ApplyBindingOverride(0, "");
            }break;
        }
    }

    private void CompleteRebind(InputActionRebindingExtensions.RebindingOperation operation)
    {
        operation.action.Enable();

        InputBinding new_binding = InputBindingFromKeyAction(m_currentRebindKeyAction);
        // HACK(ah): For some reason the display string is the correct newly remapped key
        // but the path, and doing == doesn't always work xd
        string new_str = new_binding.ToDisplayString();
        for(int i = 0; i < (int)KeyAction.COUNT; i++) 
        {
            KeyAction action     = (KeyAction)i;
            InputBinding binding = InputBindingFromKeyAction(action);
            if(action != m_currentRebindKeyAction)
            {
                string str = binding.ToDisplayString();
                if(str == new_str)
                {
                    Unbind(action);
                }
            }
        }

        m_onRebindComplete?.Invoke(m_currentRebindKeyAction);
    }

    public static string GetStringFromKeyAction(KeyAction action)
    {
        return Instance.InputBindingFromKeyAction(action).ToDisplayString();
    }

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
