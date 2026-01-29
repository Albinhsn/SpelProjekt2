using UnityEngine;


namespace LinAlg
{
    public static class LinAlg
    {
        public static Vector3 Rejection(Vector3 a, Vector3 b)
        {
            return a - Vector3.Project(a, b);
        }
        public static Vector3 Hadamard(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }
    }
}
