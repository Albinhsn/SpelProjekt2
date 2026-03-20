using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class RebindButton : MonoBehaviour
{
    [SerializeField]
    private KeyAction m_action;

    [SerializeField]
    TextMeshProUGUI m_text;

    void Awake()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(Rebind);

        UpdateText();
    }

    public void Rebind()
    {
        InputManager.StartRemap(m_action);
    }

    public void UpdateText()
    {
        string text = InputManager.GetStringFromKeyAction(m_action);
        if(text.Contains("/"))
        {
            string[] splits = text.Split("/");
            text = splits[0] + " " + splits[1];
        }
        m_text.text = text;
    }
}
