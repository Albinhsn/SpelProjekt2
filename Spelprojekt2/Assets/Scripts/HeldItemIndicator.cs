using UnityEngine;
using static LinAlg.LinAlg;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class HeldItemIndicator : MonoBehaviour
{
    
    public void Init(GameObject obj)
    {
        float r = 0;
        MeshFilter filter = obj.GetComponent<MeshFilter>();
        if(filter != null)
        {
            Bounds bounds = filter.mesh.bounds;
            r = Hadamard(bounds.size, obj.transform.localScale).magnitude;
            this.transform.localScale = new Vector3(r,r,r);
        }
        else
        {
            Debug.LogError("Expected mesh filter on held item");
            this.transform.localScale = Vector3.zero;
        }

        this.transform.SetParent(obj.transform);
        this.transform.localPosition = Vector3.zero;

    }
}
