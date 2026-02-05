using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class LevelCheckpoint : MonoBehaviour
{

    void OnTriggerEnter(Collider other)
    {
        LevelCheckpointManager.SetNewSpawnPoint(this.transform.position);
    }
}
