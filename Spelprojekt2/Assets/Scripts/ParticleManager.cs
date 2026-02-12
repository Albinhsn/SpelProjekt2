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

    public static void PlayParticleEffect(Vector3 position, Quaternion rotation, Vector3 boxSize, Material material)
    {
        Instance.m_particleEffectPrefab = Resources.Load<GameObject>("Prefabs/ParticleEffect");
        ParticleSystem ps = GameObject.Instantiate(Instance.m_particleEffectPrefab, position, rotation).GetComponent<ParticleSystem>();
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = boxSize;
        
        ps.gameObject.GetComponent<ParticleSystemRenderer>().material = new(material);

    }
}
