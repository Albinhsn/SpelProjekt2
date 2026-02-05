using UnityEngine;


[RequireComponent(typeof(BoxCollider))]
public class RespawnPlayerOnCollision : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        LevelCheckpointManager.Respawn();
    }
}
