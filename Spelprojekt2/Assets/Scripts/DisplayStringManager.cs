using UnityEngine;

public enum DisplayStringKind
{
    TutorialMovement,
    TutorialSprint,
    TutorialShoulder,
    TutorialJump,
    TutorialElevator,
    TutorialFreeLook,
    TutorialFirstPerson,
    World1FirstFilter,
    World1SecondFilter,
}

public static class DisplayStringManager
{

    public static string GetString(DisplayStringKind kind)
    {
        string result = "";
        switch(kind)
        {
            case DisplayStringKind.TutorialMovement:
            {
                string move_forward = InputManager.GetStringFromKeyAction(KeyAction.KM_Forward);
                string move_left    = InputManager.GetStringFromKeyAction(KeyAction.KM_Left);
                string move_right   = InputManager.GetStringFromKeyAction(KeyAction.KM_Right);
                string move_back    = InputManager.GetStringFromKeyAction(KeyAction.KM_Back);
                string controller_movement = InputManager.GetStringFromKeyAction(KeyAction.C_Movement);
                string km_pickup = InputManager.GetStringFromKeyAction(KeyAction.KM_Pickup);
                string c_pickup  = InputManager.GetStringFromKeyAction(KeyAction.C_Pickup);
                result = $"Use \"{move_forward},{move_left},{move_back},{move_right}\" / \"{controller_movement}\" to Move\n\n\"{km_pickup}\" / \"{c_pickup}\" to Pickup the boxes";
            }break;
            case DisplayStringKind.TutorialSprint:
            {
                string km_sprint = InputManager.GetStringFromKeyAction(KeyAction.KM_Sprint);
                string c_sprint = InputManager.GetStringFromKeyAction(KeyAction.C_Sprint);
                result = $"Hold \"{km_sprint}\" / \"{c_sprint}\" to Sprint";
            }break;
            case DisplayStringKind.TutorialShoulder:
            {
            }break;
            case DisplayStringKind.TutorialJump:
            {
                string km_jump = InputManager.GetStringFromKeyAction(KeyAction.KM_Jump);
                string c_jump = InputManager.GetStringFromKeyAction(KeyAction.C_Jump);
                result = $"Press \"{km_jump}\" / \"{c_jump}\" to jump";
            }break;
            case DisplayStringKind.TutorialElevator:
            {
            }break;
            case DisplayStringKind.TutorialFreeLook:
            {
            }break;
            case DisplayStringKind.TutorialFirstPerson:
            {
            }break;
            case DisplayStringKind.World1FirstFilter:
            {
            }break;
            case DisplayStringKind.World1SecondFilter:
            {
            }break;
        }
        return result;
    }
}
