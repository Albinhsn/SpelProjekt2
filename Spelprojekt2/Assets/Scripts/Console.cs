using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Console : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Action to reset level")]
    private InputActionReference m_resetAction;

    void ResetLevel(InputAction.CallbackContext ctx)
    {
        string current_scene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(current_scene);
    }

    void Start()
    {
        if(m_resetAction != null)
        {
            m_resetAction.action.Enable();
            m_resetAction.action.performed += ResetLevel;
        }
    }
}
