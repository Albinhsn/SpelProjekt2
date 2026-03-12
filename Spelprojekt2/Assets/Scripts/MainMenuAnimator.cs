using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuAnimator : MonoBehaviour
{
    [SerializeField] private float m_loopTime;
    [SerializeField] private float m_spawnFirstCubeTime;
    [SerializeField] private float m_activateFilterTime;
    [SerializeField] private float m_spawnSecondCubeTime;

    [SerializeField] private Spawner m_filteredSpawner;
    [SerializeField] private Spawner m_normalSpawner;

    private float t;
    private bool m_activatedFilter;
    private bool m_spawnedSecondCube;
    private bool m_spawnedFirstCube;

    private string m_sceneName;
    private FilterObject m_filterObject;
    private GameObject m_normalObject;


    void Awake()
    {
        m_sceneName     = SceneManager.GetActiveScene().name;
    }

    void CreateParticles(GameObject obj)
    {
        MeshFilter filter = obj.GetComponent<MeshFilter>();
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        ParticleManager.PlayParticleEffect(obj.transform.position, obj.transform.rotation,
                filter.sharedMesh, renderer.sharedMaterial, obj.transform.localScale, this.gameObject.scene);
    }

    void Update()
    {
        t += Time.deltaTime;

        if(t >= m_spawnFirstCubeTime && !m_spawnedFirstCube)
        {
            m_spawnedFirstCube = true;
            var spawned_filter_object = m_filteredSpawner.Spawn();
            m_filterObject = spawned_filter_object.GetComponent<FilterObject>();
        }

        if(t >= m_activateFilterTime && !m_activatedFilter)
        {
            m_activatedFilter = true;
            m_filterObject.Activate();
        }

        if(t >= m_spawnSecondCubeTime && !m_spawnedSecondCube)
        {
            m_normalObject = m_normalSpawner.Spawn();
            m_spawnedSecondCube = true;
        }

        if(t >= m_loopTime)
        {
            t = 0;
            m_filterObject.Deactivate();

            CreateParticles(m_filterObject.gameObject);
            CreateParticles(m_normalObject);
            m_filteredSpawner.Despawn();
            m_normalSpawner.Despawn();

            m_spawnedFirstCube  = false;
            m_spawnedSecondCube = false;
            m_activatedFilter   = false;

            
        }

    }

}
