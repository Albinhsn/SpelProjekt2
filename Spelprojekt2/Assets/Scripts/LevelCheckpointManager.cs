using UnityEngine;

public sealed class LevelCheckpointManager
{
    public static Vector3 m_currentSpawnPoint;
    public static int m_sceneBuildIndex;

    public static void Respawn()
    {
        Player player = Object.FindFirstObjectByType<Player>();

        if(player != null)
        {
            // TODO(ah): Figure out which levels to reload?
            player.transform.position = m_currentSpawnPoint;
        }
    }

    public static void SetNewSpawnPoint(Vector3 point, int scene_build_index)
    {
        m_currentSpawnPoint = point;
        m_sceneBuildIndex   = scene_build_index;
    }

    public static void ResetToCheckpoint()
    {
        // Delete save file for current scene
        PersistentDataManager.RemoveSceneData(m_sceneBuildIndex); 
        Respawn();

    }
}
