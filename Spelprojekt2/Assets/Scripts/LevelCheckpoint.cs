using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class LevelCheckpoint : MonoBehaviour
{

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Settings new spawn points {this.gameObject.scene.buildIndex} in scene {this.gameObject.scene.name}");
        LevelCheckpointManager.SetNewSpawnPoint(this.transform.position, this.gameObject.scene.buildIndex);
    }
}
