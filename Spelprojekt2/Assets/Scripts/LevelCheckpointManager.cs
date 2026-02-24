using UnityEngine;

public sealed class LevelCheckpointManager
{
    public static bool m_allowChangeCheckpoint = true;
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

    public static void SetFirstSpawnPoint()
    {
        LevelCheckpoint[] checkpoints = Object.FindObjectsByType<LevelCheckpoint>(FindObjectsSortMode.None);

        for(int i = 0; i < checkpoints.Length; i++)
        {
            var checkpoint = checkpoints[i];
            if(checkpoint.m_isFirstCheckpointOfLevel)
            {
                SetNewSpawnPoint(checkpoint.transform.position, checkpoint.transform.rotation,
                      checkpoint.gameObject.scene.buildIndex);
            }
        }
    }

    public static void SetNewSpawnPoint(Vector3 p, Quaternion r, int scene_build_index)
    {
        if(m_allowChangeCheckpoint)
        {
            m_currentSpawnPointPosition = p;
            m_currentSpawnPointRotation = r;
            m_sceneBuildIndex           = scene_build_index;
        }
    }
}
