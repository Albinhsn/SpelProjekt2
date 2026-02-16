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

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(245 / 255.0f, 40 / 255.0f, 145 / 255.0f, 0.8f);
        BoxCollider col = GetComponent<BoxCollider>();
        Gizmos.DrawCube(this.transform.position + col.center, col.size);

    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Settings new spawn points {this.gameObject.scene.buildIndex} in scene {this.gameObject.scene.name}");
        LevelCheckpointManager.SetNewSpawnPoint(this.transform.position, this.transform.rotation, this.gameObject.scene.buildIndex);
    }
}
