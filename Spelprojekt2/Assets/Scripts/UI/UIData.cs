using UnityEngine;

[CreateAssetMenu(fileName = "UIData", menuName = "ScriptableObjects/UIData")]
public class UIData : ScriptableObject
{
     public Font font;
     public UIState m_stateOnInitialization;
     public LevelsData m_levelData;
     public Player m_playerPrefab;
     public float m_areaWidth;
     public float m_areaHeight;
     public float m_btnWidth;
     public float m_btnHeight;
     public float m_sliderWidth;
     public Texture2D m_btnTexture;
     public Texture2D m_pauseMenuBG;
}
