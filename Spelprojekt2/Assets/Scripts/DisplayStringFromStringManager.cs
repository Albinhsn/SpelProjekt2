using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshPro))]
public class DisplayStringFromStringManager : MonoBehaviour
{
    [SerializeField]
    private DisplayStringKind m_kind;

    private TextMeshPro m_text;

    void Awake()
    {
        m_text = GetComponent<TextMeshPro>();
    }

    void FixedUpdate()
    {
        m_text.text = DisplayStringManager.GetString(m_kind);
    }
}
