using UnityEngine;

public class DestroyPlayerOnLoad : MonoBehaviour
{

    void Update()
    {
        Player player = FindFirstObjectByType<Player>();
        if(player != null)
        {
            Destroy(player.gameObject);
            Destroy(this.gameObject);
        }
    }
}
