using UnityEngine;

namespace Interaction.Dialogue
{
    [RequireComponent(typeof(SerializableObject))]
    public class InkAssetRegistry : MonoBehaviour
    {
        [SerializeField] private Entry[] m_assets;
        private int m_activeAssetIndex = 0;

        public int activeAssetIndex => m_activeAssetIndex;

        public void SetActiveAsset(string name)
        {
            for (int a = 0; a < m_assets.Length; a++)
            {
                if (m_assets[a].name == name)
                {
                    m_activeAssetIndex = a;
                    Debug.Log($"Setting active ink asset to {a}");
                    return;
                }
            }
        }

        public void SetActiveAsset(int index)
        {
            m_activeAssetIndex = index;
            Debug.Log($"Setting active ink asset to {index}");
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