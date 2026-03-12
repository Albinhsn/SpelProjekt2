using UnityEngine;
using UnityEngine.SceneManagement;
using AudioKit.FMOD;

public class ParticleManager
{
    private static ParticleManager _instance;
    private static ParticleManager Instance {
    get
        {
            if(_instance == null)
            {
                _instance = new ParticleManager();
                _instance.m_dissolveSound = Resources.Load<AudioCueSO>("Audio/AC_Dissolve");
            }
            return _instance;
        } 
    }

    private GameObject m_particleEffectPrefab;
    private AudioCueSO m_dissolveSound;

    public static void PlayParticleEffect(Vector3 position, Quaternion rotation, Mesh objMesh, Material material, Vector3 scale,
            Scene scene)
    {
        Instance.m_particleEffectPrefab = Resources.Load<GameObject>("Prefabs/ParticleEffect");

        ParticleSystem ps = GameObject.Instantiate(Instance.m_particleEffectPrefab, position, rotation).GetComponent<ParticleSystem>();
        SceneManager.MoveGameObjectToScene(ps.gameObject, scene);
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.mesh = objMesh;
        
        ps.gameObject.transform.localScale = scale;
        
        ps.gameObject.GetComponent<ParticleSystemRenderer>().material = new(material);

        // ah: play dissolve sound 
        {
            SfxDirector.PlayCue2(Instance.m_dissolveSound, position);
        }

    }
}
