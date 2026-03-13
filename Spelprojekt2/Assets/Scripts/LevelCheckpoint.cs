using UnityEngine;
using static LinAlg.LinAlg;

[RequireComponent(typeof(BoxCollider))]
public class LevelCheckpoint : MonoBehaviour
{

    [SerializeField]
    public bool m_isFirstCheckpointOfLevel;

    void Start()
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
        Gizmos.DrawCube(this.transform.position + col.center, Hadamard(col.size, this.transform.localScale));

    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            LevelCheckpointManager.SetNewSpawnPoint(this.transform.position, this.transform.rotation, this.gameObject.scene.buildIndex);
        }
    }
}
