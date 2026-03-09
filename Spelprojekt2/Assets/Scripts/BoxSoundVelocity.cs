using UnityEngine;

[CreateAssetMenu(fileName = "BoxSoundVelocity", menuName = "ScriptableObjects/BoxSoundVelocity")]
public class BoxSoundVelocity : ScriptableObject
{
    [Range(0.1f, 100.0f)]
    public float m_velocity;
}
