using Unity.VisualScripting;
using UnityEngine;

namespace srUtils.Unity
{
    public class SFXTrigger : MonoBehaviour
    {
        [SerializeField] private SFXRelay sfxRelay;
        [SerializeField] private SFXCollection clipCollection;

        private void OnValidate()
        {
            if (clipCollection.IsUnityNull()) Debug.LogError("Missing SFXCollection reference", this);
            if (sfxRelay.IsUnityNull()) Debug.LogError("Missing SFXRelay reference", this);
        }

        public void Trigger()
        {
            sfxRelay.PlayClip(clipCollection.GetRandomClip());
        }
    }
}