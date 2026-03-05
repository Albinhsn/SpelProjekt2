using System;
using UnityEngine;

namespace Interaction.Dialogue
{
    [CreateAssetMenu(menuName = "Data/DialogueSystem/ColorSet")]
    public class DialogueColorSet : ScriptableObject
    {
        [SerializeField] private Color m_alternativeDefault;
        [SerializeField] private Color m_alternativeHighlight;
        [SerializeField] private ColorEntry[] m_colors = { new ColorEntry("default", Color.white) };
        public Color textDefaultColor => m_colors[0].color;
        public Color alternativeHighlightColor => m_alternativeHighlight;
        public Color alternativeDefaultColor => m_alternativeDefault;

        private void OnValidate()
        {
            if (m_colors.Length < 1)
            {
                m_colors = new[] { new ColorEntry("default", Color.white) };
            }

            m_colors[0].name = "default";
        }

        public Color GetColor(string key)
        {
            for (int a = 0; a < m_colors.Length; a++)
            {
                if (key == m_colors[a].name) return m_colors[a].color;
            }
            return m_colors[0].color;
        }
        

        [System.Serializable]
        public struct ColorEntry
        {
            public ColorEntry(string name, Color color)
            {
                this.name = name;
                this.color = color;
            }
            public string name;
            public Color color;
        }
    }
}