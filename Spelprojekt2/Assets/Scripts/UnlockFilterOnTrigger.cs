using UnityEngine;
using AudioKit.FMOD;

[RequireComponent(typeof(Collider))]
public class UnlockFilterOnTrigger : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioCueSO m_pickupCue;

    public void Unlock()
    {
        FilterManager fm = FindFirstObjectByType<FilterManager>();
        fm.Unlock();

        if (m_pickupCue != null)
        {
            SfxDirector.PlayCue2(m_pickupCue, transform.position);
        }

        Material material = GetComponent<MeshRenderer>().material;
        ParticleManager.PlayParticleEffect(transform.position, transform.rotation, gameObject.GetComponent<Mesh>(), material, new Vector3(.5f,.5f,.5f), this.gameObject.scene);
        Destroy(gameObject);
    }

    private void ActivateOutline(Collider other)
    {
        PlayerPowerUpAnimation player = other.GetComponentInChildren<PlayerPowerUpAnimation>();
        if(player != null) player.ActivateOutlineAnimation();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Unlock();
            ActivateOutline(other);
        }
    }
}