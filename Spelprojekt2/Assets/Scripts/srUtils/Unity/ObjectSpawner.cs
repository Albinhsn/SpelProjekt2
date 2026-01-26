using UnityEngine;

namespace srUtils.Unity
{
    public class ObjectSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        public void Spawn()
        {
            InstantiateAsync(prefab, transform.position, Quaternion.identity);
        }
    }
}