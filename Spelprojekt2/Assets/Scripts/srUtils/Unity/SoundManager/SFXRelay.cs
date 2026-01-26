using System;
using UnityEngine;

namespace srUtils.Unity
{
    [CreateAssetMenu (menuName = "SFX/relay")]
    public class SFXRelay : ScriptableObject
    {
        private Action<SoundEffectData> output;

        public void PlayClip(SoundEffectData clip)
        {
            output.Invoke(clip);
        }

        public void RegisterReciever(Action<SoundEffectData> action)
        {
            output += action;
        }
    }
}