using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class LevelCheckpoint : MonoBehaviour
{

    [SerializeField]
    private bool m_isFirstCheckpointOfLevel;

    void Awake()
    {
        if(m_isFirstCheckpointOfLevel)
        {
            LevelCheckpointManager.SetNewSpawnPoint(this.transform.position, this.transform.rotation, this.gameObject.scene.buildIndex);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Settings new spawn points {this.gameObject.scene.buildIndex} in scene {this.gameObject.scene.name}");
        LevelCheckpointManager.SetNewSpawnPoint(this.transform.position, this.transform.rotation, this.gameObject.scene.buildIndex);
    }
}
