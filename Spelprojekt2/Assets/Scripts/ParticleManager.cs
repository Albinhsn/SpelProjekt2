using UnityEngine;

public class ParticleManager
{
    private static ParticleManager _instance;
    private static ParticleManager Instance {
    get
        {
            if(_instance == null)
            {
                _instance = new ParticleManager();
            }
            return _instance;
        } 
    }

    private GameObject m_particleEffectPrefab;

    public static void PlayParticleEffect(Vector3 position, Quaternion rotation, Mesh objMesh, Material material, Vector3 scale)
    {
        Instance.m_particleEffectPrefab = Resources.Load<GameObject>("Prefabs/ParticleEffect");
        ParticleSystem ps = GameObject.Instantiate(Instance.m_particleEffectPrefab, position, rotation).GetComponent<ParticleSystem>();
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.mesh = objMesh;
        
        ps.gameObject.transform.localScale = scale;
        
        ps.gameObject.GetComponent<ParticleSystemRenderer>().material = new(material);

    }
}
