using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace srUtils.Unity
{
    public class SFXController : MonoBehaviour
    {

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private SFXRelay relay;
        private List<AudioClip> effectLockList = new List<AudioClip>();
        private List<float> effectLockTime = new List<float>();

        private void Awake()
        {
            relay.RegisterReciever(Play);
        }

        private void OnValidate()
        {
            if (audioSource.IsUnityNull()) Debug.LogError("Missing audioSource reference", this);
            if (relay.IsUnityNull()) Debug.LogError("Missing SFXRelay reference", this);
        }

        private void FixedUpdate()
        {
            for (int a = effectLockTime.Count - 1; a >= 0; a--)
            {
                effectLockTime[a] -= Time.deltaTime;
                if (effectLockTime[a] <= 0)
                {
                    effectLockList.RemoveAt(a);
                    effectLockTime.RemoveAt(a);
                }
            }
        }

        private void Play(SoundEffectData sfxData)
        {
            if (effectLockList.Contains(sfxData.audioClip)) return;

            effectLockList.Add(sfxData.audioClip);
            effectLockTime.Add(sfxData.minRepeatDelay);
            audioSource.PlayOneShot(sfxData.audioClip);

        }

    }
}