using UnityEngine;

public sealed class LevelCheckpointManager
{
    public static Vector3 m_currentSpawnPoint;

    public static void Respawn()
    {
        Player player = Object.FindFirstObjectByType<Player>();

        if(player != null)
        {
            // TODO(ah): Figure out which levels to reload?
            player.transform.position = m_currentSpawnPoint;
        }
    }

    public static void SetNewSpawnPoint(Vector3 point)
    {
        m_currentSpawnPoint = point;
    }
}
