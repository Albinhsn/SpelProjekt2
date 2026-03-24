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
                string km_shoulder = InputManager.GetStringFromKeyAction(KeyAction.KM_CameraChangeShoulder); 
                string c_shoulder = InputManager.GetStringFromKeyAction(KeyAction.C_CameraChangeShoulder); 
                result = $"Press \"{km_shoulder}\" / \"{c_shoulder}\" to move the camera shoulder";
            }break;
            case DisplayStringKind.TutorialJump:
            {
                string km_jump = InputManager.GetStringFromKeyAction(KeyAction.KM_Jump);
                string c_jump = InputManager.GetStringFromKeyAction(KeyAction.C_Jump);
                result = $"Press \"{km_jump}\" / \"{c_jump}\" to jump";
            }break;
            case DisplayStringKind.TutorialElevator:
            {
                string km_interaction = InputManager.GetStringFromKeyAction(KeyAction.KM_Interaction);
                string c_interaction = InputManager.GetStringFromKeyAction(KeyAction.C_Interaction);
                result = $"Interact with buttons on walls\nand in elevators with \"{km_interaction}\" / \"{c_interaction}\"";
            }break;
            case DisplayStringKind.TutorialFreeLook:
            {
                string km_free_look = InputManager.GetStringFromKeyAction(KeyAction.KM_CameraFreeLookToggle);
                string c_free_look = InputManager.GetStringFromKeyAction(KeyAction.C_CameraFreeLookToggle);
                result = $"Hold \"{km_free_look}\" / \"{c_free_look}\" to toggle freelook camera";
            }break;
            case DisplayStringKind.TutorialFirstPerson:
            {
                string km_first_person = InputManager.GetStringFromKeyAction(KeyAction.KM_CameraFirstPerson);
                string c_first_person = InputManager.GetStringFromKeyAction(KeyAction.C_CameraFirstPerson);
                result = $"Hold \"{km_first_person}\" / \"{c_first_person}\" to go into first person While in first person, use the \"Scrollwheel\" / \"Left Stick Up and down\" to zoom";
            }break;
            case DisplayStringKind.World1FirstFilter:
            {
                string km_filter = InputManager.GetStringFromKeyAction(KeyAction.KM_PrimaryFilter);
                string c_filter = InputManager.GetStringFromKeyAction(KeyAction.C_PrimaryFilter);
                result = $"Press \"{km_filter}\" / \"{c_filter}\" To activate filter";
            }break;
            case DisplayStringKind.World1SecondFilter:
            {
                string km_filter = InputManager.GetStringFromKeyAction(KeyAction.KM_SecondaryFilter);
                string c_filter = InputManager.GetStringFromKeyAction(KeyAction.C_SecondaryFilter);
                result = $"Press \"{km_filter}\" / \"{c_filter}\" To activate filter";
            }break;
        }
        return result;
    }
}
