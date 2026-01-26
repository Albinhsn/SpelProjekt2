using UnityEngine;

namespace srUtils.Unity
{
    [CreateAssetMenu(menuName = "SFX/collection")]
    public class SFXCollection : ScriptableObject
    {
        [SerializeField] private SoundEffectData[] collection;

        public SoundEffectData GetRandomClip()
        {
            return collection[new System.Random().Next(collection.Length)];
        }
    }
}