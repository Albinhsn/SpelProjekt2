using UnityEngine;

public sealed class LevelCheckpointManager
{
    public static Vector3 m_currentSpawnPointPosition;
    public static Quaternion m_currentSpawnPointRotation;
    public static int m_sceneBuildIndex;

    public static void Respawn()
    {
        Player player = Object.FindFirstObjectByType<Player>();

        if(player != null)
        {
            player.transform.position = m_currentSpawnPointPosition;
            player.transform.rotation = m_currentSpawnPointRotation;
        }
    }

    public static void SetNewSpawnPoint(Vector3 p, Quaternion r, int scene_build_index)
    {
        m_currentSpawnPointPosition = p;
        m_currentSpawnPointRotation = r;
        m_sceneBuildIndex   = scene_build_index;
    }

    public static void ResetToCheckpoint()
    {
        // Delete save file for current scene
        PersistentDataManager.RemoveSceneData(m_sceneBuildIndex); 
        Respawn();

    }
}
