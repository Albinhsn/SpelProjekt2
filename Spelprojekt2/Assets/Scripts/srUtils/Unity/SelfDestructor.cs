using UnityEngine;

namespace srUtils.Unity
{
    public class SelfDestructor : MonoBehaviour
    {
        public void Destroy() => Destroy(gameObject);
    }
}