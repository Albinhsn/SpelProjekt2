using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace Interaction.Dialogue
{
    [CreateAssetMenu (menuName = "Data/DialogueSystem/FontSet")]
    public class FontSet : ScriptableObject
    {
        [SerializeField] private TMP_FontAsset[] m_fonts;
        public int count => m_fonts.Length;

        public TMP_FontAsset GetFont(int index) => m_fonts[index];

        public bool TryGetFont(int index, [CanBeNull] TMP_FontAsset output)
        {
            if (index >= count) return false;
            output = m_fonts[index];
            return true;
        }
    }
}