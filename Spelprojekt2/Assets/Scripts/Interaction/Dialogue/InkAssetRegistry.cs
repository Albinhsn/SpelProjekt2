using UnityEngine;

namespace Interaction.Dialogue
{
    public class InkAssetRegistry : MonoBehaviour
    {
        [SerializeField] private Entry[] m_assets;
        private int m_activeAssetIndex = 0;

        public void SetActiveAsset(string name)
        {
            for (int a = 0; a < m_assets.Length; a++)
            {
                if (m_assets[a].name == name)
                {
                    m_activeAssetIndex = a;
                }
            }
        }

        public TextAsset activeInkAsset => m_assets[m_activeAssetIndex].inkAsset;

        [System.Serializable]
        private class Entry
        {
            [SerializeField] private string m_name;
            [SerializeField] private TextAsset m_inkAsset;
            public string name => m_name;
            public TextAsset inkAsset => m_inkAsset;
        }
    }
}